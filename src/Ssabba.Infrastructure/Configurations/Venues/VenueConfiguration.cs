using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ssabba.Domain.Entities;

namespace Ssabba.Infrastructure.Configurations;


public class VenueConfiguration : IEntityTypeConfiguration<Venue>
{
    public void Configure(EntityTypeBuilder<Venue> builder)
    {
        builder.Property(v => v.Name).HasMaxLength(160).IsRequired();
        builder.Property(v => v.Address).HasMaxLength(400);
        builder.Property(v => v.Notes).HasMaxLength(2000);
        builder.Property(v => v.OpeningHours).HasColumnType("jsonb");

        builder.HasIndex(v => v.OwnerCommunityId);

        builder.HasOne(v => v.OwnerCommunity)
            .WithMany()
            .HasForeignKey(v => v.OwnerCommunityId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
