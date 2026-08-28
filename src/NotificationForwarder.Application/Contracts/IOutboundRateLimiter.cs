using System;

namespace NotificationForwarder.Application.Contracts;

public interface IOutboundRateLimiter
{
    bool TryAcquire();
}
