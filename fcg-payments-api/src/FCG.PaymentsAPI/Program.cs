using FCG.PaymentsAPI.Consumers;
using FCG.PaymentsAPI.Services;
using MassTransit;
using MongoDB.Driver;
using Prometheus;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSingleton<PaymentSimulator>();

// MongoDB
var mongoUri = builder.Configuration.GetConnectionString("MongoDB") ?? "mongodb://admin:admin@localhost:27017";
var mongoClient = new MongoClient(mongoUri);
var mongoDatabase = mongoClient.GetDatabase("fcg_db");
builder.Services.AddSingleton(mongoDatabase);

// Prometheus
builder.Services.AddSingleton(CollectorRegistry.Default);

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<OrderPlacedConsumer>();

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

var host = builder.Build();
host.Run();
