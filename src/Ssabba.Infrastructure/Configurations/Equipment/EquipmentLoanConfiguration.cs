using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ssabba.Domain.Entities;

namespace Ssabba.Infrastructure.Configurations;


public class EquipmentLoanConfiguration : IEntityTypeConfiguration<EquipmentLoan>
{
    public void Configure(EntityTypeBuilder<EquipmentLoan> builder)
    {
        builder.Property(l => l.Note).HasMaxLength(500);

        builder.HasIndex(l => new { l.MemberId, l.ReturnedAt });

        // An item is out on loan to at most one person at a time.
        builder.HasIndex(l => l.EquipmentItemId)
            .IsUnique()
            .HasFilter("\"ReturnedAt\" IS NULL")
            .HasDatabaseName("IX_EquipmentLoans_EquipmentItemId_Outstanding");

        builder.HasOne(l => l.EquipmentItem)
            .WithMany()
            .HasForeignKey(l => l.EquipmentItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(l => l.Member)
            .WithMany()
            .HasForeignKey(l => l.MemberId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(l => l.Session)
            .WithMany()
            .HasForeignKey(l => l.SessionId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
