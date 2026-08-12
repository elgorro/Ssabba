using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ssabba.Domain.Entities;

namespace Ssabba.Infrastructure.Configurations;


public class ConsentRecordConfiguration : IEntityTypeConfiguration<ConsentRecord>
{
    public void Configure(EntityTypeBuilder<ConsentRecord> builder)
    {
        builder.Property(c => c.PolicyVersion).HasMaxLength(40);
        builder.Property(c => c.Source).HasMaxLength(200);

        // Reading current consent means taking the newest row for a player and kind.
        builder.HasIndex(c => new { c.PlayerId, c.Kind, c.RecordedAt });

        builder.HasOne(c => c.Player)
            .WithMany()
            .HasForeignKey(c => c.PlayerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.Community)
            .WithMany()
            .HasForeignKey(c => c.CommunityId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
