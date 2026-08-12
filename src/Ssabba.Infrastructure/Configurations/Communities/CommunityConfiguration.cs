using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ssabba.Domain.Entities;

namespace Ssabba.Infrastructure.Configurations;


public class CommunityConfiguration : IEntityTypeConfiguration<Community>
{
    public void Configure(EntityTypeBuilder<Community> builder)
    {
        builder.Property(c => c.Name).HasMaxLength(160).IsRequired();
        builder.Property(c => c.Slug).HasMaxLength(120).IsRequired();
        builder.Property(c => c.Description).HasMaxLength(2000);
        builder.Property(c => c.TimeZone).HasMaxLength(64).IsRequired();
        builder.Property(c => c.Currency).HasMaxLength(3).IsFixedLength().IsRequired();

        builder.HasIndex(c => c.Slug).IsUnique();
        builder.HasIndex(c => c.PublicKeyId).IsUnique();
    }
}
