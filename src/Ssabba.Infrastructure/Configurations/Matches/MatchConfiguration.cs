using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ssabba.Domain.Entities;

namespace Ssabba.Infrastructure.Configurations;


public class MatchConfiguration : IEntityTypeConfiguration<Match>
{
    public void Configure(EntityTypeBuilder<Match> builder)
    {
        builder.Property(m => m.LocationNote).HasMaxLength(200);

        builder.HasIndex(m => m.PlayedAt);
        builder.HasIndex(m => new { m.CommunityId, m.PlayedAt });

        // The rating worker's queue: confirmed but not yet counted.
        builder.HasIndex(m => new { m.CommunityId, m.Status, m.RatingAppliedAt });

        builder.Ignore(m => m.HomeSetsWon);
        builder.Ignore(m => m.AwaySetsWon);
        builder.Ignore(m => m.Outcome);

        builder.HasOne(m => m.Community)
            .WithMany()
            .HasForeignKey(m => m.CommunityId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(m => m.Format)
            .WithMany()
            .HasForeignKey(m => m.FormatId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.Season)
            .WithMany()
            .HasForeignKey(m => m.SeasonId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(m => m.Session)
            .WithMany()
            .HasForeignKey(m => m.SessionId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(m => m.Court)
            .WithMany()
            .HasForeignKey(m => m.CourtId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(m => m.RuleSet)
            .WithMany()
            .HasForeignKey(m => m.RuleSetId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(m => m.RecordedByMember)
            .WithMany()
            .HasForeignKey(m => m.RecordedByMemberId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(m => m.HomeTeam)
            .WithMany()
            .HasForeignKey(m => m.HomeTeamId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.AwayTeam)
            .WithMany()
            .HasForeignKey(m => m.AwayTeamId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(m => m.Sets)
            .WithOne(s => s.Match)
            .HasForeignKey(s => s.MatchId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(m => m.Appearances)
            .WithOne(a => a.Match)
            .HasForeignKey(a => a.MatchId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
