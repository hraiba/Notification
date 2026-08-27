namespace NotificationForwarder.Application.Contracts;

public interface IDiscordNotifier
{
    Task NotifyAsync(string message, CancellationToken cancellationToken = default);
}