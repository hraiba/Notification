using System.Threading.RateLimiting;
using NotificationForwarder.Application.Contracts;

namespace NotificationForwarder.Infrastructure;

public class OutboundRateLimiter : IOutboundRateLimiter, IDisposable
{
    private readonly SlidingWindowRateLimiter _rateLimiter = new(new SlidingWindowRateLimiterOptions
    {
        PermitLimit = 10,
        Window = TimeSpan.FromMinutes(1),
        SegmentsPerWindow = 1,
        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
        QueueLimit = 0
    });

    public void Dispose() => _rateLimiter.Dispose();

    public bool TryAcquire()
    {
        using var lastLease = _rateLimiter.AttemptAcquire();
        return lastLease.IsAcquired;
    }
}
