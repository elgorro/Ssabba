using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ssabba.Domain.Entities;

namespace Ssabba.Infrastructure.Configurations;


public class TournamentConfiguration : IEntityTypeConfiguration<Tournament>
{
    public void Configure(EntityTypeBuilder<Tournament> builder)
    {
        builder.Property(t => t.Name).HasMaxLength(160).IsRequired();

        builder.HasIndex(t => new { t.CommunityId, t.StartsOn });

        builder.HasOne(t => t.Community)
            .WithMany()
            .HasForeignKey(t => t.CommunityId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(t => t.Format)
            .WithMany()
            .HasForeignKey(t => t.FormatId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.Season)
            .WithMany()
            .HasForeignKey(t => t.SeasonId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(t => t.Venue)
            .WithMany()
            .HasForeignKey(t => t.VenueId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(t => t.RuleSet)
            .WithMany()
            .HasForeignKey(t => t.RuleSetId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(t => t.Matches)
            .WithOne(m => m.Tournament)
            .HasForeignKey(m => m.TournamentId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
