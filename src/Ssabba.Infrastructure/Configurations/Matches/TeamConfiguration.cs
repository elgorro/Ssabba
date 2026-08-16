using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ssabba.Domain;
using Ssabba.Domain.Entities;

namespace Ssabba.Infrastructure.Configurations;


public class TeamConfiguration : IEntityTypeConfiguration<Team>
{
    public void Configure(EntityTypeBuilder<Team> builder)
    {
        builder.Property(t => t.Name).HasMaxLength(120);

        builder.Property(t => t.MemberKey).IsRequired().HasMaxLength(TeamRoster.MaxKeyLength);

        builder.HasIndex(t => new { t.CommunityId, t.IsAdHoc });

        // One row per lineup per community: teams are looked up by their roster before being created.
        builder.HasIndex(t => new { t.CommunityId, t.MemberKey }).IsUnique();

        builder.HasOne(t => t.Community)
            .WithMany()
            .HasForeignKey(t => t.CommunityId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
