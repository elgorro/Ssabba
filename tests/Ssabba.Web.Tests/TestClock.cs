namespace Ssabba.Web.Tests;

/// <summary>
/// The clock, under the test's control. The correction window is a rule about elapsed time, and the
/// only other way to test it is to wait.
/// </summary>
public sealed class TestClock : TimeProvider
{
    private DateTimeOffset now = DateTimeOffset.UtcNow;

    public override DateTimeOffset GetUtcNow() => now;

    /// <summary>Moves the clock forward, as the hours after a game do.</summary>
    public void Advance(TimeSpan by) => now += by;
}
