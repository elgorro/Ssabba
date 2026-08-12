using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ssabba.Domain.Entities;

namespace Ssabba.Infrastructure.Configurations;


public class MediaAssetConfiguration : IEntityTypeConfiguration<MediaAsset>
{
    public void Configure(EntityTypeBuilder<MediaAsset> builder)
    {
        builder.Property(m => m.StoragePath).HasMaxLength(500).IsRequired();
        builder.Property(m => m.ContentType).HasMaxLength(120).IsRequired();
        builder.Property(m => m.Sha256).HasMaxLength(64);

        builder.HasIndex(m => new { m.CommunityId, m.CreatedAt });
        builder.HasIndex(m => m.Sha256).HasFilter("\"Sha256\" IS NOT NULL");

        builder.HasOne(m => m.Community)
            .WithMany()
            .HasForeignKey(m => m.CommunityId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(m => m.UploadedByMember)
            .WithMany()
            .HasForeignKey(m => m.UploadedByMemberId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
