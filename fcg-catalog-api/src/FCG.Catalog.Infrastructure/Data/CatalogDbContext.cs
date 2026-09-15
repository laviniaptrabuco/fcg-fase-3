using FCG.Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FCG.Catalog.Infrastructure.Data;

public class CatalogDbContext : DbContext
{
    public CatalogDbContext(DbContextOptions<CatalogDbContext> options) : base(options) { }

    public DbSet<Game> Games => Set<Game>();
    public DbSet<UserLibrary> UserLibraries => Set<UserLibrary>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CatalogDbContext).Assembly);
    }
}
