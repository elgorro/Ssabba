using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ssabba.Domain.Entities;

namespace Ssabba.Infrastructure.Configurations;


public class SessionConfiguration : IEntityTypeConfiguration<Session>
{
    public void Configure(EntityTypeBuilder<Session> builder)
    {
        builder.Property(s => s.Title).HasMaxLength(160).IsRequired();
        builder.Property(s => s.Notes).HasMaxLength(2000);
        builder.Property(s => s.CancellationReason).HasMaxLength(500);

        // The calendar query: this community, upcoming, still alive.
        builder.HasIndex(s => new { s.CommunityId, s.StartsAt });
        builder.HasIndex(s => new { s.CommunityId, s.Status });

        // A recurring fixture generates one session per occurrence, never two.
        builder.HasIndex(s => new { s.TemplateId, s.StartsAt })
            .IsUnique()
            .HasFilter("\"TemplateId\" IS NOT NULL");

        builder.HasOne(s => s.Community)
            .WithMany()
            .HasForeignKey(s => s.CommunityId)
            .OnDelete(DeleteBehavior.Cascade);

        // Dropping a recurring fixture must not take its history with it.
        builder.HasOne(s => s.Template)
            .WithMany()
            .HasForeignKey(s => s.TemplateId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(s => s.Court)
            .WithMany()
            .HasForeignKey(s => s.CourtId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(s => s.RuleSet)
            .WithMany()
            .HasForeignKey(s => s.RuleSetId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(s => s.OrganizerMember)
            .WithMany()
            .HasForeignKey(s => s.OrganizerMemberId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
