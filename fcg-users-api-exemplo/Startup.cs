using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using StackExchange.Redis;
using Prometheus;

namespace FcgUsersApi
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            // MongoDB
            var mongoUri = Configuration.GetConnectionString("MongoDB") ?? "mongodb://admin:admin@localhost:27017";
            var mongoClient = new MongoClient(mongoUri);
            var mongoDatabase = mongoClient.GetDatabase("fcg_db");
            services.AddSingleton(mongoDatabase);

            // Redis
            var redisConnection = Configuration.GetConnectionString("Redis") ?? "localhost:6379";
            var redis = ConnectionMultiplexer.Connect(redisConnection);
            services.AddSingleton(redis);
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnection;
            });

            // Prometheus Metrics
            services.AddSingleton<ICollectorRegistry>(CollectorRegistry.Default);

            // Controllers
            services.AddControllers();
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", builder =>
                {
                    builder.AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader();
                });
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // Prometheus Middleware
            app.UseHttpMetrics();

            app.UseRouting();
            app.UseCors("AllowAll");

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();

                // Expose metrics for Prometheus
                endpoints.MapMetrics("/metrics");
            });
        }
    }
}
