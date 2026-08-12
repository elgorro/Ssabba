using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ssabba.Domain.Entities;

namespace Ssabba.Infrastructure.Configurations;


public class MatchAppearanceConfiguration : IEntityTypeConfiguration<MatchAppearance>
{
    public void Configure(EntityTypeBuilder<MatchAppearance> builder)
    {
        // A player appears once in a match, on one side.
        builder.HasIndex(a => new { a.MatchId, a.PlayerId }).IsUnique();

        // A player's rating history, newest first.
        builder.HasIndex(a => new { a.MemberId, a.MatchId });

        builder.HasOne(a => a.Player)
            .WithMany()
            .HasForeignKey(a => a.PlayerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.Member)
            .WithMany()
            .HasForeignKey(a => a.MemberId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
