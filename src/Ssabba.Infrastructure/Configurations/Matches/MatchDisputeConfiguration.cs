using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ssabba.Domain.Entities;

namespace Ssabba.Infrastructure.Configurations;


public class MatchDisputeConfiguration : IEntityTypeConfiguration<MatchDispute>
{
    public void Configure(EntityTypeBuilder<MatchDispute> builder)
    {
        builder.Property(d => d.Reason).HasMaxLength(1000).IsRequired();
        builder.Property(d => d.Resolution).HasMaxLength(1000);

        builder.HasIndex(d => new { d.MatchId, d.Status });

        builder.HasOne(d => d.Match)
            .WithMany()
            .HasForeignKey(d => d.MatchId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(d => d.RaisedByMember)
            .WithMany()
            .HasForeignKey(d => d.RaisedByMemberId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(d => d.ResolvedByMember)
            .WithMany()
            .HasForeignKey(d => d.ResolvedByMemberId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
