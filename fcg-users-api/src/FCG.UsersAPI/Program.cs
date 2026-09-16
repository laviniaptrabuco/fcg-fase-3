using System.Text;
using FCG.Users.Application.Services;
using FCG.Users.Infrastructure.Data;
using FCG.Users.Infrastructure.Extensions;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using StackExchange.Redis;
using Prometheus;
using FCG.Users.Infrastructure.NoSql;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<FCG.Users.Application.Services.UserService>();

// MongoDB
var mongoUri = builder.Configuration.GetConnectionString("MongoDB") ?? "mongodb://admin:admin@localhost:27017";
var mongoClient = new MongoClient(mongoUri);
var mongoDatabase = mongoClient.GetDatabase("fcg_db");
builder.Services.AddSingleton(mongoDatabase);

// Redis
var redisConnection = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";
var redis = ConnectionMultiplexer.Connect(redisConnection);
builder.Services.AddSingleton(redis);
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConnection;
});

// Repositório NoSQL de perfis (MongoDB + cache-aside em Redis)
builder.Services.AddScoped<UserProfileRepository>();

var jwtSecret = builder.Configuration["Jwt:Secret"]
    ?? throw new InvalidOperationException("Jwt:Secret is not configured.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMQ:Host"] ?? "localhost", "/", h =>
        {
            h.Username(builder.Configuration["RabbitMQ:Username"] ?? "guest");
            h.Password(builder.Configuration["RabbitMQ:Password"] ?? "guest");
        });
        cfg.ConfigureEndpoints(context);
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "FCG Users API",
        Version = "v1",
        Description = "Microsserviço responsável por cadastro, autenticação e autorização de usuários."
    });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Informe o token JWT no formato: Bearer {seu_token}"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddControllers();

// Desabilita infer de body em minimal APIs - fix para o erro
// "Body was inferred but the method does not allow inferred body parameters"
builder.Services.Configure<ApiBehaviorOptions>(opts =>
{
    opts.SuppressInferBindingSourcesForParameters = true;
});

var app = builder.Build();

// EnsureCreated() com retry: o container do SQL Server pode estar de pe
// (porta aceitando TCP) mas ainda inicializando internamente, o que
// derruba a primeira tentativa de conexao. O K8s/compose ja aguardam o
// healthcheck do banco antes de iniciar este container, mas isso da uma
// segunda camada de seguranca contra esse tipo de corrida.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<UsersDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    const int maxAttempts = 8;
    for (var attempt = 1; attempt <= maxAttempts; attempt++)
    {
        try
        {
            db.Database.EnsureCreated();
            break;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Falha ao criar/verificar o banco (tentativa {Attempt}/{Max})", attempt, maxAttempts);
            if (attempt < maxAttempts) Thread.Sleep(5000);
        }
    }
}

app.UseMiddleware<FCG.UsersAPI.Middleware.ErrorHandlingMiddleware>();

// Prometheus metrics middleware
app.UseHttpMetrics();

app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "FCG Users API v1"));

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Prometheus endpoints
app.MapMetrics("/metrics");

// Health check
app.MapGet("/health", async (IMongoDatabase mongoDb) =>
{
    try
    {
        await mongoDb.RunCommandAsync<BsonDocument>(new BsonDocument("ping", 1));
        return Results.Ok(new
        {
            status = "healthy",
            mongodb = "connected",
            timestamp = DateTime.UtcNow
        });
    }
    catch (Exception ex)
    {
        return Results.Json(new { status = "unhealthy", error = ex.Message }, statusCode: 503);
    }
}).AllowAnonymous();

app.Run();

public partial class Program { }
