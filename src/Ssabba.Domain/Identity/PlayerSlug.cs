using System.Text;

namespace Ssabba.Domain.Identity;

/// <summary>
/// Turns a display name into the URL-safe identifier stored on <see cref="Entities.Player.Slug"/>.
/// Uniqueness is the database's business; this only decides the shape.
/// </summary>
public static class PlayerSlug
{
    /// <summary>Matches the length of the <c>Slug</c> column.</summary>
    public const int MaxLength = 120;

    // The app runs with InvariantGlobalization, where string.Normalize is a no-op, so accented
    // letters are folded by hand. Latin-1 and the common Latin Extended-A letters are enough for
    // the languages this is likely to see; anything else falls back to a separator.
    private const string Accented = "àáâãäåāăąÀÁÂÃÄÅĀĂĄçćĉčÇĆĈČèéêëēĕėęěÈÉÊËĒĔĖĘĚìíîïĩīįÌÍÎÏĨĪĮñńņňÑŃŅŇòóôõöøōŏőÒÓÔÕÖØŌŎŐřŕŘŔšśşŠŚŞťţŤŢùúûüũūŭůűÙÚÛÜŨŪŬŮŰýÿŷÝŸŶžźżŽŹŻğĝĞĜ";
    private const string Folded = "aaaaaaaaaAAAAAAAAAccccCCCCeeeeeeeeeEEEEEEEEEiiiiiiiIIIIIIInnnnNNNNoooooooooOOOOOOOOOrrRRsssSSStsTTuuuuuuuuuUUUUUUUUUyyyYYYzzzZZZggGG";

    /// <summary>
    /// Lowercases, folds accented letters onto their base letter ("Jürgen" becomes "jurgen") and
    /// reduces everything else to single hyphens. Returns an empty string when nothing usable is left.
    /// </summary>
    public static string From(string displayName)
    {
        ArgumentNullException.ThrowIfNull(displayName);

        var builder = new StringBuilder(displayName.Length);

        foreach (var character in displayName)
        {
            if (char.IsAsciiLetterOrDigit(character))
            {
                builder.Append(char.ToLowerInvariant(character));

                continue;
            }

            // Not a decomposable letter, and dropping it would mangle a common German name.
            if (character is 'ß')
            {
                builder.Append("ss");

                continue;
            }

            var accent = Accented.IndexOf(character, StringComparison.Ordinal);

            if (accent >= 0)
            {
                builder.Append(Folded[accent]);
            }
            else if (builder.Length > 0 && builder[^1] != '-')
            {
                builder.Append('-');
            }
        }

        // A trailing separator can only be the one appended above, so trimming it is enough.
        var slug = builder.ToString().TrimEnd('-');

        return slug.Length > MaxLength ? slug[..MaxLength].TrimEnd('-') : slug;
    }
}
