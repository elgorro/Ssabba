namespace Ssabba.TestSupport;

/// <summary>
/// Trait values used to select tiers of the suite. Everything that needs Docker carries
/// <see cref="Integration"/> so `dotnet test --filter "Category!=Integration"` still runs on a
/// machine without a container runtime.
/// </summary>
public static class TestCategories
{
    public const string Category = "Category";
    public const string Integration = "Integration";
}
