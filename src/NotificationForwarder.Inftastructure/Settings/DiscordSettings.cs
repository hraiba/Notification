namespace NotificationForwarder.Infrastructure.Settings;

public record DiscordSettings
{
    public string WebhookUrl { get; init; } = string.Empty;
}