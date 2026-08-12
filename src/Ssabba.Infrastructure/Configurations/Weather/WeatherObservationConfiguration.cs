using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ssabba.Domain.Entities;

namespace Ssabba.Infrastructure.Configurations;


public class WeatherObservationConfiguration : IEntityTypeConfiguration<WeatherObservation>
{
    public void Configure(EntityTypeBuilder<WeatherObservation> builder)
    {
        builder.Property(o => o.Provider).HasMaxLength(60).IsRequired();
        builder.Property(o => o.ConditionCode).HasMaxLength(60);
        builder.Property(o => o.ConditionText).HasMaxLength(200);

        // One observation per session: this is the record of what it was actually like.
        builder.HasIndex(o => o.SessionId).IsUnique();

        builder.HasOne(o => o.Session)
            .WithMany()
            .HasForeignKey(o => o.SessionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
