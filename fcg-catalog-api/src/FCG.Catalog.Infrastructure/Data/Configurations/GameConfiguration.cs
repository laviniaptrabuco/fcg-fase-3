using FCG.Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FCG.Catalog.Infrastructure.Data.Configurations;

public class GameConfiguration : IEntityTypeConfiguration<Game>
{
    public void Configure(EntityTypeBuilder<Game> builder)
    {
        builder.ToTable("Games");
        builder.HasKey(g => g.Id);
        builder.Property(g => g.Title).IsRequired().HasMaxLength(200);
        builder.HasIndex(g => g.Title).IsUnique();
        builder.Property(g => g.Description).HasMaxLength(2000);
        builder.Property(g => g.Genre).IsRequired().HasMaxLength(100);
        builder.Property(g => g.Price).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(g => g.PromotionalPrice).HasColumnType("decimal(18,2)");
        builder.Property(g => g.CreatedAt).IsRequired();
        builder.Property(g => g.IsActive).IsRequired().HasDefaultValue(true);
    }
}
