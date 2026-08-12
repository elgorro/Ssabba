using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ssabba.Domain.Entities;

namespace Ssabba.Infrastructure.Configurations;


public class PollOptionConfiguration : IEntityTypeConfiguration<PollOption>
{
    public void Configure(EntityTypeBuilder<PollOption> builder)
    {
        builder.Property(o => o.Label).HasMaxLength(200).IsRequired();

        builder.HasIndex(o => new { o.PollId, o.SortOrder });

        builder.HasOne(o => o.Poll)
            .WithMany(p => p.Options)
            .HasForeignKey(o => o.PollId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(o => o.Court)
            .WithMany()
            .HasForeignKey(o => o.CourtId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
