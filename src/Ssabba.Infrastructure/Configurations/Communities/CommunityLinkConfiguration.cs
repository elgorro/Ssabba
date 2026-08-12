using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ssabba.Domain.Entities;

namespace Ssabba.Infrastructure.Configurations;


public class CommunityLinkConfiguration : IEntityTypeConfiguration<CommunityLink>
{
    public void Configure(EntityTypeBuilder<CommunityLink> builder)
    {
        builder.Property(l => l.TargetCommunityUri).HasMaxLength(400).IsRequired();
        builder.Property(l => l.TargetName).HasMaxLength(160);
        builder.Property(l => l.SharedSecretHash).HasMaxLength(128);

        builder.HasIndex(l => new { l.SourceCommunityId, l.TargetCommunityUri }).IsUnique();

        builder.HasOne(l => l.SourceCommunity)
            .WithMany()
            .HasForeignKey(l => l.SourceCommunityId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
