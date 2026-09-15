using FCG.Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FCG.Catalog.Infrastructure.Data.Configurations;

public class UserLibraryConfiguration : IEntityTypeConfiguration<UserLibrary>
{
    public void Configure(EntityTypeBuilder<UserLibrary> builder)
    {
        builder.ToTable("UserLibraries");
        builder.HasKey(ul => new { ul.UserId, ul.GameId });
        builder.Property(ul => ul.AcquiredAt).IsRequired();

        builder.HasOne(ul => ul.Game)
            .WithMany()
            .HasForeignKey(ul => ul.GameId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
