using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ssabba.Domain.Entities;

namespace Ssabba.Infrastructure.Configurations;


public class PlayerConfiguration : IEntityTypeConfiguration<Player>
{
    public void Configure(EntityTypeBuilder<Player> builder)
    {
        builder.Property(p => p.DisplayName).HasMaxLength(120).IsRequired();
        builder.Property(p => p.Slug).HasMaxLength(120).IsRequired();
        builder.Property(p => p.SubjectId).HasMaxLength(200);
        builder.Property(p => p.PreferredTimeZone).HasMaxLength(64);
        builder.Property(p => p.Locale).HasMaxLength(16);

        // Slugs and subjects are unique among the living; an erased player must not block reuse.
        builder.HasIndex(p => p.Slug).IsUnique().HasFilter("\"DeletedAt\" IS NULL");
        builder.HasIndex(p => p.SubjectId).IsUnique().HasFilter("\"SubjectId\" IS NOT NULL AND \"DeletedAt\" IS NULL");

        builder.HasOne(p => p.Profile)
            .WithOne(x => x.Player)
            .HasForeignKey<PlayerProfile>(x => x.PlayerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
