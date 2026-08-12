using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ssabba.Domain.Entities;

namespace Ssabba.Infrastructure.Configurations;


public class CommunityMemberConfiguration : IEntityTypeConfiguration<CommunityMember>
{
    public void Configure(EntityTypeBuilder<CommunityMember> builder)
    {
        builder.Property(m => m.Nickname).HasMaxLength(120);

        builder.HasIndex(m => new { m.CommunityId, m.PlayerId }).IsUnique();

        // The ladder: highest rated first, within one community.
        builder.HasIndex(m => new { m.CommunityId, m.Rating });

        builder.HasOne(m => m.Community)
            .WithMany(c => c.Members)
            .HasForeignKey(m => m.CommunityId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(m => m.Player)
            .WithMany(p => p.Memberships)
            .HasForeignKey(m => m.PlayerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
