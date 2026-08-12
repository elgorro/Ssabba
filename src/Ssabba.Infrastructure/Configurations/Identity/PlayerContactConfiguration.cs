using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ssabba.Domain.Entities;

namespace Ssabba.Infrastructure.Configurations;


public class PlayerContactConfiguration : IEntityTypeConfiguration<PlayerContact>
{
    public void Configure(EntityTypeBuilder<PlayerContact> builder)
    {
        builder.Property(c => c.Value).HasMaxLength(320).IsRequired();
        builder.Property(c => c.Label).HasMaxLength(60);

        builder.HasIndex(c => new { c.PlayerId, c.Kind, c.Value }).IsUnique();

        builder.HasOne(c => c.Player)
            .WithMany(p => p.Contacts)
            .HasForeignKey(c => c.PlayerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
