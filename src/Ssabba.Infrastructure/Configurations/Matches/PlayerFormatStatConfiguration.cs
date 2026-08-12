using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ssabba.Domain.Entities;

namespace Ssabba.Infrastructure.Configurations;


public class PlayerFormatStatConfiguration : IEntityTypeConfiguration<PlayerFormatStat>
{
    public void Configure(EntityTypeBuilder<PlayerFormatStat> builder)
    {
        // One row per member, format and season. Postgres treats NULLs as distinct, so the all-time
        // row needs its own constraint to stay singular.
        builder.HasIndex(s => new { s.MemberId, s.FormatId, s.SeasonId })
            .IsUnique()
            .HasFilter("\"SeasonId\" IS NOT NULL");

        builder.HasIndex(s => new { s.MemberId, s.FormatId })
            .IsUnique()
            .HasFilter("\"SeasonId\" IS NULL")
            .HasDatabaseName("IX_PlayerFormatStats_MemberId_FormatId_AllTime");

        builder.HasOne(s => s.Member)
            .WithMany()
            .HasForeignKey(s => s.MemberId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(s => s.Format)
            .WithMany()
            .HasForeignKey(s => s.FormatId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Season)
            .WithMany()
            .HasForeignKey(s => s.SeasonId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
