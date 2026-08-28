namespace NotificationForwarder.Application.Contracts;

public interface IDiscordNotifier
{
    Task Notify(string message, CancellationToken cancellationToken = default);
}