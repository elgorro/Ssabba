namespace Ssabba.Domain.Entities;

/// <summary>
/// What the weather actually did at a session. Forecasts are fetched live and not kept; this is the
/// snapshot taken at the time, which is the only one still worth anything afterwards.
/// </summary>
public class WeatherObservation
{
    public Guid Id { get; set; } = Guid.CreateVersion7();

    /// <summary>The session this describes. At most one observation per session.</summary>
    public Guid SessionId { get; set; }
    public Session? Session { get; set; }

    public required string Provider { get; set; }

    public DateTimeOffset ObservedAt { get; set; }

    public DateTimeOffset FetchedAt { get; set; } = DateTimeOffset.UtcNow;

    public double? TemperatureC { get; set; }

    public double? FeelsLikeC { get; set; }

    public double? WindKph { get; set; }

    public double? WindGustKph { get; set; }

    public double? PrecipitationMm { get; set; }

    public int? CloudPercent { get; set; }

    public int? HumidityPercent { get; set; }

    /// <summary>The provider's own condition code, kept verbatim rather than normalised away.</summary>
    public string? ConditionCode { get; set; }

    public string? ConditionText { get; set; }
}
