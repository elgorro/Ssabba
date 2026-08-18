namespace Ssabba.Shared;

/// <summary>Endpoint paths shared by the minimal API and the WASM client.</summary>
public static class ApiRoutes
{
    public const string Matches = "/api/matches";
    public const string Teams = "/api/teams";
    public const string Players = "/api/players";

    /// <summary>Singular: one instance, one community.</summary>
    public const string Community = "/api/community";
}
