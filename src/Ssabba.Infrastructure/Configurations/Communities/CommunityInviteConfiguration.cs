using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ssabba.Domain.Entities;

namespace Ssabba.Infrastructure.Configurations;


public class CommunityInviteConfiguration : IEntityTypeConfiguration<CommunityInvite>
{
    public void Configure(EntityTypeBuilder<CommunityInvite> builder)
    {
        builder.Property(i => i.TokenHash).HasMaxLength(128).IsRequired();
        builder.Property(i => i.Email).HasMaxLength(320);

        builder.HasIndex(i => i.TokenHash).IsUnique();
        builder.HasIndex(i => new { i.CommunityId, i.ExpiresAt });

        builder.HasOne(i => i.Community)
            .WithMany()
            .HasForeignKey(i => i.CommunityId)
            .OnDelete(DeleteBehavior.Cascade);

        // Keep the invite readable after the inviting member is gone.
        builder.HasOne(i => i.InvitedByMember)
            .WithMany()
            .HasForeignKey(i => i.InvitedByMemberId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.AcceptedByPlayer)
            .WithMany()
            .HasForeignKey(i => i.AcceptedByPlayerId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
