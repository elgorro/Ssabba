using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ssabba.Domain.Entities;

namespace Ssabba.Infrastructure.Configurations;

public class MatchConfiguration : IEntityTypeConfiguration<Match>
{
    public void Configure(EntityTypeBuilder<Match> builder)
    {
        builder.Property(m => m.Location).HasMaxLength(200);
        builder.HasIndex(m => m.PlayedAt);

        builder.Ignore(m => m.HomeSetsWon);
        builder.Ignore(m => m.AwaySetsWon);
        builder.Ignore(m => m.Outcome);

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
    }
}

public class MatchSetConfiguration : IEntityTypeConfiguration<MatchSet>
{
    public void Configure(EntityTypeBuilder<MatchSet> builder)
    {
        builder.HasIndex(s => new { s.MatchId, s.Number }).IsUnique();
    }
}

public class TournamentConfiguration : IEntityTypeConfiguration<Tournament>
{
    public void Configure(EntityTypeBuilder<Tournament> builder)
    {
        builder.Property(t => t.Name).HasMaxLength(160).IsRequired();

        builder.HasMany(t => t.Matches)
            .WithOne(m => m.Tournament)
            .HasForeignKey(m => m.TournamentId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
