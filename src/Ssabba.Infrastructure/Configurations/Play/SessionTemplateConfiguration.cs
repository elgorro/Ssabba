using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ssabba.Domain.Entities;

namespace Ssabba.Infrastructure.Configurations;


public class SessionTemplateConfiguration : IEntityTypeConfiguration<SessionTemplate>
{
    public void Configure(EntityTypeBuilder<SessionTemplate> builder)
    {
        builder.Property(t => t.Title).HasMaxLength(160).IsRequired();
        builder.Property(t => t.Rrule).HasMaxLength(400).IsRequired();

        builder.HasIndex(t => new { t.CommunityId, t.IsActive });

        builder.HasOne(t => t.Community)
            .WithMany()
            .HasForeignKey(t => t.CommunityId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(t => t.Court)
            .WithMany()
            .HasForeignKey(t => t.CourtId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(t => t.DefaultRuleSet)
            .WithMany()
            .HasForeignKey(t => t.DefaultRuleSetId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
