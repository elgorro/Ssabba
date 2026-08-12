using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ssabba.Domain.Entities;

namespace Ssabba.Infrastructure.Configurations;


public class RuleSetConfiguration : IEntityTypeConfiguration<RuleSet>
{
    public void Configure(EntityTypeBuilder<RuleSet> builder)
    {
        builder.Property(r => r.Name).HasMaxLength(120).IsRequired();
        builder.Property(r => r.Notes).HasMaxLength(2000);

        builder.HasIndex(r => new { r.CommunityId, r.FormatId });

        // One default per community and format.
        builder.HasIndex(r => new { r.CommunityId, r.FormatId })
            .IsUnique()
            .HasFilter("\"IsDefault\"")
            .HasDatabaseName("IX_RuleSets_CommunityId_FormatId_Default");

        builder.HasOne(r => r.Community)
            .WithMany()
            .HasForeignKey(r => r.CommunityId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.Format)
            .WithMany()
            .HasForeignKey(r => r.FormatId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
