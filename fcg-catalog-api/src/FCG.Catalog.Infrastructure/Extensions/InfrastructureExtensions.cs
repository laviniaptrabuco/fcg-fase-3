using FCG.Catalog.Domain.Interfaces;
using FCG.Catalog.Infrastructure.Data;
using FCG.Catalog.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FCG.Catalog.Infrastructure.Extensions;

public static class InfrastructureExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<CatalogDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sql => sql.MigrationsAssembly(typeof(CatalogDbContext).Assembly.FullName)));

        services.AddScoped<IGameRepository, GameRepository>();
        services.AddScoped<IUserLibraryRepository, UserLibraryRepository>();

        return services;
    }
}
