using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ssabba.Domain.Entities;

namespace Ssabba.Infrastructure.Configurations;


public class CourtReservationConfiguration : IEntityTypeConfiguration<CourtReservation>
{
    public void Configure(EntityTypeBuilder<CourtReservation> builder)
    {
        builder.Property(r => r.Note).HasMaxLength(500);
        builder.Property(r => r.Currency).HasMaxLength(3).IsFixedLength();

        builder.HasIndex(r => new { r.CourtId, r.StartsAt });

        // Overlap is barred by an exclusion constraint in the migration, not from here: application
        // checks race, and EF cannot express EXCLUDE.
        builder.HasOne(r => r.Court)
            .WithMany()
            .HasForeignKey(r => r.CourtId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.HeldByCommunity)
            .WithMany()
            .HasForeignKey(r => r.HeldByCommunityId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(r => r.HeldByMember)
            .WithMany()
            .HasForeignKey(r => r.HeldByMemberId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
