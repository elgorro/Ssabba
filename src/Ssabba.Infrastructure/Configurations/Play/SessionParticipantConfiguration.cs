using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ssabba.Domain.Entities;

namespace Ssabba.Infrastructure.Configurations;


public class SessionParticipantConfiguration : IEntityTypeConfiguration<SessionParticipant>
{
    public void Configure(EntityTypeBuilder<SessionParticipant> builder)
    {
        builder.Property(p => p.Note).HasMaxLength(500);

        builder.HasIndex(p => new { p.SessionId, p.MemberId }).IsUnique();

        builder.HasOne(p => p.Session)
            .WithMany(s => s.Participants)
            .HasForeignKey(p => p.SessionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.Member)
            .WithMany()
            .HasForeignKey(p => p.MemberId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.IsGuestOfMember)
            .WithMany()
            .HasForeignKey(p => p.IsGuestOfMemberId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
