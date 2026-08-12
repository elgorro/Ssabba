using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ssabba.Domain.Entities;

namespace Ssabba.Infrastructure.Configurations;


public class TeamConfiguration : IEntityTypeConfiguration<Team>
{
    public void Configure(EntityTypeBuilder<Team> builder)
    {
        builder.Property(t => t.Name).HasMaxLength(120);

        builder.HasIndex(t => new { t.CommunityId, t.IsAdHoc });

        builder.HasOne(t => t.Community)
            .WithMany()
            .HasForeignKey(t => t.CommunityId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
