using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ssabba.Domain.Entities;

namespace Ssabba.Infrastructure.Configurations;


public class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.Property(a => a.Name).HasMaxLength(120).IsRequired();
        builder.Property(a => a.Currency).HasMaxLength(3).IsFixedLength().IsRequired();

        builder.HasIndex(a => new { a.CommunityId, a.Kind });

        // One balance account per member.
        builder.HasIndex(a => a.MemberId).IsUnique().HasFilter("\"MemberId\" IS NOT NULL");

        builder.HasOne(a => a.Community)
            .WithMany()
            .HasForeignKey(a => a.CommunityId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.Member)
            .WithMany()
            .HasForeignKey(a => a.MemberId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
