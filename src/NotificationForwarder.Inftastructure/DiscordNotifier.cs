using System;
using NotificationForwarder.Application.Contracts;

namespace NotificationForwarder.Infrastructure;

public sealed class DiscordNotifier : IDiscordNotifier
{
    public Task NotifyAsync(string message, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"Sending notification to Discord: {message}");
        return Task.CompletedTask;
    }
}
