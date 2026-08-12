using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ssabba.Domain.Entities;

namespace Ssabba.Infrastructure.Configurations;


public class JoinRequestConfiguration : IEntityTypeConfiguration<JoinRequest>
{
    public void Configure(EntityTypeBuilder<JoinRequest> builder)
    {
        builder.Property(r => r.Message).HasMaxLength(1000);

        // One open request per player per community; decided ones may accumulate.
        builder.HasIndex(r => new { r.CommunityId, r.PlayerId })
            .IsUnique()
            .HasFilter("\"Status\" = 0")
            .HasDatabaseName("IX_JoinRequests_CommunityId_PlayerId_Pending");

        builder.HasOne(r => r.Community)
            .WithMany()
            .HasForeignKey(r => r.CommunityId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.Player)
            .WithMany()
            .HasForeignKey(r => r.PlayerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.DecidedByMember)
            .WithMany()
            .HasForeignKey(r => r.DecidedByMemberId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
