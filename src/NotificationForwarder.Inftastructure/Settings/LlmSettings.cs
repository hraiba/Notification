namespace NotificationForwarder.Infrastructure.Settings;

public record LlmSettings
{
    public const string SectionName = "LLM";
    public string Endpoint { get; init; } = "http://localhost:11434/v1/chat/completions";
    public string ApiKey { get; init; } = string.Empty;
    public string Model { get; init; } = "gemma4:e2b";
}
