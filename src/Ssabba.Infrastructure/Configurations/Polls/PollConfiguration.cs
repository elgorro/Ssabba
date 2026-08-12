using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ssabba.Domain.Entities;

namespace Ssabba.Infrastructure.Configurations;


public class PollConfiguration : IEntityTypeConfiguration<Poll>
{
    public void Configure(EntityTypeBuilder<Poll> builder)
    {
        builder.Property(p => p.Question).HasMaxLength(500).IsRequired();

        builder.HasIndex(p => new { p.CommunityId, p.Status });

        builder.HasOne(p => p.Community)
            .WithMany()
            .HasForeignKey(p => p.CommunityId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.CreatedByMember)
            .WithMany()
            .HasForeignKey(p => p.CreatedByMemberId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.ResultSession)
            .WithMany()
            .HasForeignKey(p => p.ResultSessionId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
