using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ssabba.Domain.Entities;

namespace Ssabba.Infrastructure.Configurations;


public class EquipmentItemConfiguration : IEntityTypeConfiguration<EquipmentItem>
{
    public void Configure(EntityTypeBuilder<EquipmentItem> builder)
    {
        builder.Property(e => e.Name).HasMaxLength(120).IsRequired();
        builder.Property(e => e.AssetTag).HasMaxLength(60);
        builder.Property(e => e.Notes).HasMaxLength(2000);
        builder.Property(e => e.Currency).HasMaxLength(3).IsFixedLength();

        builder.HasIndex(e => new { e.CommunityId, e.Status });
        builder.HasIndex(e => new { e.CommunityId, e.AssetTag })
            .IsUnique()
            .HasFilter("\"AssetTag\" IS NOT NULL");

        builder.HasOne(e => e.Community)
            .WithMany()
            .HasForeignKey(e => e.CommunityId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.HomeVenue)
            .WithMany()
            .HasForeignKey(e => e.HomeVenueId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
