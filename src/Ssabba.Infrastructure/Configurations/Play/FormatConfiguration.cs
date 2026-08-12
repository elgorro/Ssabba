using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ssabba.Domain.Entities;

namespace Ssabba.Infrastructure.Configurations;

public class FormatConfiguration : IEntityTypeConfiguration<Format>
{
    /// <summary>
    /// Fixed identifiers, because these rows are seeded into every database and are referenced by
    /// rule sets and matches. They must be identical everywhere, so they cannot be generated.
    /// </summary>
    public static readonly Guid TwoVsTwo = new("0195f000-0000-7000-8000-000000000002");
    public static readonly Guid ThreeVsThree = new("0195f000-0000-7000-8000-000000000003");
    public static readonly Guid FourVsFour = new("0195f000-0000-7000-8000-000000000004");
    public static readonly Guid FiveVsFive = new("0195f000-0000-7000-8000-000000000005");
    public static readonly Guid SixVsSix = new("0195f000-0000-7000-8000-000000000006");

    public void Configure(EntityTypeBuilder<Format> builder)
    {
        builder.Property(f => f.Name).HasMaxLength(40).IsRequired();

        builder.HasIndex(f => f.Code).IsUnique();

        builder.HasData(
            // Beach proper: short sets, and every point is yours to win or lose.
            New(TwoVsTwo, FormatCode.TwoVsTwo, "2v2", pointsPerSet: 21, tiebreak: 15, weight: 100),
            New(ThreeVsThree, FormatCode.ThreeVsThree, "3v3", pointsPerSet: 21, tiebreak: 15, weight: 85),
            New(FourVsFour, FormatCode.FourVsFour, "4v4", pointsPerSet: 21, tiebreak: 15, weight: 70),
            // Full sides play the indoor distance.
            New(FiveVsFive, FormatCode.FiveVsFive, "5v5", pointsPerSet: 25, tiebreak: 15, weight: 60),
            New(SixVsSix, FormatCode.SixVsSix, "6v6", pointsPerSet: 25, tiebreak: 15, weight: 50));
    }

    private static Format New(Guid id, FormatCode code, string name, int pointsPerSet, int tiebreak, int weight) =>
        new()
        {
            Id = id,
            Code = code,
            PlayersPerSide = (int)code,
            Name = name,
            DefaultSetsToWin = 2,
            DefaultPointsPerSet = pointsPerSet,
            DefaultWinBy = 2,
            DefaultTiebreakPoints = tiebreak,
            RatingWeightPercent = weight,
        };
}
