namespace NotificationForwarder.Infrastructure.Settings;

public record DiscordSettings
{
    public const string SectionName = "Discord";
    public string WebhookUrl { get; init; } = string.Empty;
}