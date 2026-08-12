using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ssabba.Domain.Entities;

namespace Ssabba.Infrastructure.Configurations;


public class TournamentEntryConfiguration : IEntityTypeConfiguration<TournamentEntry>
{
    public void Configure(EntityTypeBuilder<TournamentEntry> builder)
    {
        builder.HasIndex(e => new { e.TournamentId, e.TeamId }).IsUnique();

        builder.HasOne(e => e.Tournament)
            .WithMany(t => t.Entries)
            .HasForeignKey(e => e.TournamentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Team)
            .WithMany()
            .HasForeignKey(e => e.TeamId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
