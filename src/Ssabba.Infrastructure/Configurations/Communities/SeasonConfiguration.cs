using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ssabba.Domain.Entities;

namespace Ssabba.Infrastructure.Configurations;


public class SeasonConfiguration : IEntityTypeConfiguration<Season>
{
    public void Configure(EntityTypeBuilder<Season> builder)
    {
        builder.Property(s => s.Name).HasMaxLength(120).IsRequired();

        builder.HasIndex(s => new { s.CommunityId, s.StartsOn });

        // At most one current season per community, enforced by the database rather than by hope.
        builder.HasIndex(s => s.CommunityId)
            .IsUnique()
            .HasFilter("\"IsCurrent\"")
            .HasDatabaseName("IX_Seasons_CommunityId_Current");

        builder.HasOne(s => s.Community)
            .WithMany(c => c.Seasons)
            .HasForeignKey(s => s.CommunityId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
