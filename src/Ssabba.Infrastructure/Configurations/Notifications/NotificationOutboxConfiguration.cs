using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ssabba.Domain.Entities;

namespace Ssabba.Infrastructure.Configurations;


public class NotificationOutboxConfiguration : IEntityTypeConfiguration<NotificationOutbox>
{
    public void Configure(EntityTypeBuilder<NotificationOutbox> builder)
    {
        builder.Property(o => o.Payload).HasColumnType("jsonb");
        builder.Property(o => o.LastError).HasMaxLength(2000);

        // The drain query: due, not yet sent, not given up on.
        builder.HasIndex(o => o.ScheduledFor)
            .HasFilter("\"SentAt\" IS NULL AND \"AbandonedAt\" IS NULL")
            .HasDatabaseName("IX_NotificationOutbox_Pending");

        builder.HasOne(o => o.Community)
            .WithMany()
            .HasForeignKey(o => o.CommunityId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(o => o.RecipientPlayer)
            .WithMany()
            .HasForeignKey(o => o.RecipientPlayerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
