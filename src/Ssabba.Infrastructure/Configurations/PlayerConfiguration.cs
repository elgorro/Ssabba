using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ssabba.Domain.Entities;

namespace Ssabba.Infrastructure.Configurations;

public class PlayerConfiguration : IEntityTypeConfiguration<Player>
{
    public void Configure(EntityTypeBuilder<Player> builder)
    {
        builder.Property(p => p.DisplayName).HasMaxLength(120).IsRequired();
        builder.Property(p => p.SubjectId).HasMaxLength(200);
        builder.HasIndex(p => p.SubjectId).IsUnique().HasFilter("\"SubjectId\" IS NOT NULL");
    }
}
