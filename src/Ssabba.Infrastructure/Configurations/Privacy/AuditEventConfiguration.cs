using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ssabba.Domain.Entities;

namespace Ssabba.Infrastructure.Configurations;


public class AuditEventConfiguration : IEntityTypeConfiguration<AuditEvent>
{
    public void Configure(EntityTypeBuilder<AuditEvent> builder)
    {
        builder.Property(a => a.Action).HasMaxLength(100).IsRequired();
        builder.Property(a => a.EntityType).HasMaxLength(100).IsRequired();
        builder.Property(a => a.IpHash).HasMaxLength(64);
        builder.Property(a => a.Data).HasColumnType("jsonb");

        builder.HasIndex(a => new { a.CommunityId, a.OccurredAt });
        builder.HasIndex(a => new { a.EntityType, a.EntityId });

        // The audit trail outlives the community and the actor it describes.
        builder.HasOne(a => a.Community)
            .WithMany()
            .HasForeignKey(a => a.CommunityId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(a => a.ActorPlayer)
            .WithMany()
            .HasForeignKey(a => a.ActorPlayerId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
