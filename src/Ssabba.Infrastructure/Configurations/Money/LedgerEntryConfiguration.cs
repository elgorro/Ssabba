using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ssabba.Domain.Entities;

namespace Ssabba.Infrastructure.Configurations;


public class LedgerEntryConfiguration : IEntityTypeConfiguration<LedgerEntry>
{
    public void Configure(EntityTypeBuilder<LedgerEntry> builder)
    {
        builder.Property(e => e.Description).HasMaxLength(300).IsRequired();
        builder.Property(e => e.Currency).HasMaxLength(3).IsFixedLength().IsRequired();

        builder.HasIndex(e => new { e.CommunityId, e.OccurredAt });
        builder.HasIndex(e => new { e.CommunityId, e.Category });

        // Nothing that money has been booked against may be deleted out from under the books.
        builder.HasOne(e => e.DebitAccount)
            .WithMany()
            .HasForeignKey(e => e.DebitAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.CreditAccount)
            .WithMany()
            .HasForeignKey(e => e.CreditAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Community)
            .WithMany()
            .HasForeignKey(e => e.CommunityId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Session)
            .WithMany()
            .HasForeignKey(e => e.SessionId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(e => e.EquipmentItem)
            .WithMany()
            .HasForeignKey(e => e.EquipmentItemId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(e => e.ServiceRequest)
            .WithMany()
            .HasForeignKey(e => e.ServiceRequestId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(e => e.FundingSource)
            .WithMany()
            .HasForeignKey(e => e.FundingSourceId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(e => e.CreatedByMember)
            .WithMany()
            .HasForeignKey(e => e.CreatedByMemberId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(e => e.ReceiptMedia)
            .WithMany()
            .HasForeignKey(e => e.ReceiptMediaId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(e => e.ReversesEntry)
            .WithMany()
            .HasForeignKey(e => e.ReversesEntryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
