using Ssabba.Domain.Identity;

namespace Ssabba.Domain.Tests;

public class PlayerSlugTests
{
    [Theory]
    [InlineData("Ada Lovelace", "ada-lovelace")]
    [InlineData("Jürgen Müller", "jurgen-muller")]
    [InlineData("  spaced   out  ", "spaced-out")]
    [InlineData("O'Brien, Seán", "o-brien-sean")]
    [InlineData("Straße", "strasse")]
    [InlineData("Player #3", "player-3")]
    [InlineData("---", "")]
    [InlineData("", "")]
    public void From_shapes_a_url_safe_slug(string displayName, string expected) =>
        Assert.Equal(expected, PlayerSlug.From(displayName));

    [Fact]
    public void From_caps_the_slug_at_the_column_length()
    {
        var slug = PlayerSlug.From(new string('a', 200));

        Assert.Equal(PlayerSlug.MaxLength, slug.Length);
    }

    [Fact]
    public void From_never_ends_on_a_separator_after_truncation()
    {
        // The cut lands exactly on the hyphen this name produces.
        var slug = PlayerSlug.From(new string('a', PlayerSlug.MaxLength) + " tail");

        Assert.EndsWith("a", slug);
    }
}
