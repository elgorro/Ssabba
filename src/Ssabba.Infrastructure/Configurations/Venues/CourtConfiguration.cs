using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ssabba.Domain.Entities;

namespace Ssabba.Infrastructure.Configurations;


public class CourtConfiguration : IEntityTypeConfiguration<Court>
{
    public void Configure(EntityTypeBuilder<Court> builder)
    {
        builder.Property(c => c.Name).HasMaxLength(60).IsRequired();

        builder.HasIndex(c => new { c.VenueId, c.Name }).IsUnique();

        builder.HasOne(c => c.Venue)
            .WithMany(v => v.Courts)
            .HasForeignKey(c => c.VenueId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
