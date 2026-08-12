using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ssabba.Domain.Entities;

namespace Ssabba.Infrastructure.Configurations;


public class DuesAssignmentConfiguration : IEntityTypeConfiguration<DuesAssignment>
{
    public void Configure(EntityTypeBuilder<DuesAssignment> builder)
    {
        builder.Property(d => d.WaivedReason).HasMaxLength(500);

        // A member is charged once per plan per due date.
        builder.HasIndex(d => new { d.DuesPlanId, d.MemberId, d.DueOn }).IsUnique();
        builder.HasIndex(d => new { d.MemberId, d.Status });

        builder.HasOne(d => d.DuesPlan)
            .WithMany()
            .HasForeignKey(d => d.DuesPlanId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(d => d.Member)
            .WithMany()
            .HasForeignKey(d => d.MemberId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(d => d.PaidLedgerEntry)
            .WithMany()
            .HasForeignKey(d => d.PaidLedgerEntryId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
