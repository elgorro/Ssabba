using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ssabba.Domain.Entities;

namespace Ssabba.Infrastructure.Configurations;


public class FundingSourceConfiguration : IEntityTypeConfiguration<FundingSource>
{
    public void Configure(EntityTypeBuilder<FundingSource> builder)
    {
        builder.Property(f => f.Name).HasMaxLength(160).IsRequired();
        builder.Property(f => f.ContactDetails).HasMaxLength(500);
        builder.Property(f => f.Notes).HasMaxLength(2000);
        builder.Property(f => f.Currency).HasMaxLength(3).IsFixedLength();

        builder.HasIndex(f => new { f.CommunityId, f.Status });

        builder.HasOne(f => f.Community)
            .WithMany()
            .HasForeignKey(f => f.CommunityId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(f => f.ContactPlayer)
            .WithMany()
            .HasForeignKey(f => f.ContactPlayerId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(f => f.LogoMedia)
            .WithMany()
            .HasForeignKey(f => f.LogoMediaId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
