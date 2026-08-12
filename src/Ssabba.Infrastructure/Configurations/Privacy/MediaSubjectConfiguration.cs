using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ssabba.Domain.Entities;

namespace Ssabba.Infrastructure.Configurations;


public class MediaSubjectConfiguration : IEntityTypeConfiguration<MediaSubject>
{
    public void Configure(EntityTypeBuilder<MediaSubject> builder)
    {
        builder.HasKey(s => new { s.MediaAssetId, s.PlayerId });

        // Withdrawing photo consent asks this question: what is this player in?
        builder.HasIndex(s => s.PlayerId);

        builder.HasOne(s => s.MediaAsset)
            .WithMany(m => m.Subjects)
            .HasForeignKey(s => s.MediaAssetId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(s => s.Player)
            .WithMany()
            .HasForeignKey(s => s.PlayerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(s => s.TaggedByMember)
            .WithMany()
            .HasForeignKey(s => s.TaggedByMemberId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
