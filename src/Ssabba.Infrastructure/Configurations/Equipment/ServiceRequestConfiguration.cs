using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ssabba.Domain.Entities;

namespace Ssabba.Infrastructure.Configurations;


public class ServiceRequestConfiguration : IEntityTypeConfiguration<ServiceRequest>
{
    public void Configure(EntityTypeBuilder<ServiceRequest> builder)
    {
        builder.Property(r => r.Subject).HasMaxLength(200).IsRequired();
        builder.Property(r => r.Description).HasMaxLength(4000);

        builder.HasIndex(r => new { r.CommunityId, r.Status, r.Priority });

        builder.HasOne(r => r.Community)
            .WithMany()
            .HasForeignKey(r => r.CommunityId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.EquipmentItem)
            .WithMany()
            .HasForeignKey(r => r.EquipmentItemId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(r => r.Court)
            .WithMany()
            .HasForeignKey(r => r.CourtId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(r => r.RaisedByMember)
            .WithMany()
            .HasForeignKey(r => r.RaisedByMemberId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.AssignedToMember)
            .WithMany()
            .HasForeignKey(r => r.AssignedToMemberId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(r => r.CostLedgerEntry)
            .WithMany()
            .HasForeignKey(r => r.CostLedgerEntryId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
