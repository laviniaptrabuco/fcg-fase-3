using System.Text;
using FCG.Catalog.Application.Services;
using FCG.Catalog.Infrastructure.Data;
using FCG.Catalog.Infrastructure.Extensions;
using FCG.Catalog.Infrastructure.Messaging;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using StackExchange.Redis;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddScoped<GameService>();
builder.Services.AddScoped<PurchaseService>();

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

// Prometheus Metrics
builder.Services.AddSingleton<ICollectorRegistry>(CollectorRegistry.Default);

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
    x.AddConsumer<PaymentProcessedConsumer>();

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
        Title = "FCG Catalog API",
        Version = "v1",
        Description = "Microsserviço de catálogo de jogos e fluxo de compra da FIAP Cloud Games."
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

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
    db.Database.EnsureCreated();
}

app.UseMiddleware<FCG.CatalogAPI.Middleware.ErrorHandlingMiddleware>();

// Prometheus metrics middleware
app.UseHttpMetrics();

app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "FCG Catalog API v1"));

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Prometheus endpoints
app.MapMetrics("/metrics");

// Health check
app.MapGet("/health", async (IMongoDatabase mongoDb, IConnectionMultiplexer redis) =>
{
    try
    {
        await mongoDb.RunCommandAsync(new { ping = 1 });
        var pong = await redis.GetDatabase().PingAsync();

        return Results.Ok(new
        {
            status = "healthy",
            mongodb = "connected",
            redis = pong.IsNull ? "disconnected" : "connected",
            timestamp = DateTime.UtcNow
        });
    }
    catch (Exception ex)
    {
        return Results.StatusCode(503).WithOpenApi();
    }
});

app.Run();

public partial class Program { }
