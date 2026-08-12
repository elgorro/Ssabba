using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ssabba.Domain.Entities;

namespace Ssabba.Infrastructure.Configurations;


public class DuesPlanConfiguration : IEntityTypeConfiguration<DuesPlan>
{
    public void Configure(EntityTypeBuilder<DuesPlan> builder)
    {
        builder.Property(d => d.Name).HasMaxLength(120).IsRequired();
        builder.Property(d => d.Currency).HasMaxLength(3).IsFixedLength().IsRequired();

        builder.HasIndex(d => new { d.CommunityId, d.IsActive });

        builder.HasOne(d => d.Community)
            .WithMany()
            .HasForeignKey(d => d.CommunityId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(d => d.Season)
            .WithMany()
            .HasForeignKey(d => d.SeasonId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
