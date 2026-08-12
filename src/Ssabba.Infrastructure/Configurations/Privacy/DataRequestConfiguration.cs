using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ssabba.Domain.Entities;

namespace Ssabba.Infrastructure.Configurations;


public class DataRequestConfiguration : IEntityTypeConfiguration<DataRequest>
{
    public void Configure(EntityTypeBuilder<DataRequest> builder)
    {
        builder.Property(r => r.Note).HasMaxLength(1000);

        builder.HasIndex(r => new { r.PlayerId, r.Status });

        builder.HasOne(r => r.Player)
            .WithMany()
            .HasForeignKey(r => r.PlayerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.ResultMedia)
            .WithMany()
            .HasForeignKey(r => r.ResultMediaId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
