using NotificationForwarder.Infrastructure;

namespace NotificationForwarder.Tests;

public sealed class OutboundRateLimiterTests
{
    [Fact]
    public void TryAcquire_AllowsTenMessagesAndRejectsTheEleventh()
    {
        using var limiter = new OutboundRateLimiter();

        var results = Enumerable.Range(0, 11).Select(_ => limiter.TryAcquire()).ToArray();

        Assert.All(results[..10], Assert.True);
        Assert.False(results[10]);
    }
}
