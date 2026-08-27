using System;
using Microsoft.Extensions.Options;
using NotificationForwarder.Application.Contracts;
using NotificationForwarder.Infrastructure.Settings;

namespace NotificationForwarder.Infrastructure;

public sealed class DiscordNotifier(
    HttpClient httpClient,
    IOptions<DiscordSettings> options
) : IDiscordNotifier
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly DiscordSettings _settings = options.Value;
    public async Task NotifyAsync(string message, CancellationToken cancellationToken = default)
    {
        var webhookUrl = _settings.WebhookUrl;
        ArgumentNullException.ThrowIfNull(webhookUrl, nameof(webhookUrl));
        var payload = new { content = message };
        var response = await _httpClient.PostAsJsonAsync(webhookUrl, payload, cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}
