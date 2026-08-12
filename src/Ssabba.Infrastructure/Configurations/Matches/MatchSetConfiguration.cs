using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ssabba.Domain.Entities;

namespace Ssabba.Infrastructure.Configurations;


public class MatchSetConfiguration : IEntityTypeConfiguration<MatchSet>
{
    public void Configure(EntityTypeBuilder<MatchSet> builder)
    {
        builder.HasIndex(s => new { s.MatchId, s.Number }).IsUnique();
    }
}
