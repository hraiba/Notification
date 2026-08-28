using System;
using System.Text.Json;
using Microsoft.Extensions.Options;
using NotificationForwarder.Application.Contracts;
using NotificationForwarder.Application.Models;
using NotificationForwarder.Infrastructure.Settings;

namespace NotificationForwarder.Infrastructure;

public class OpenAiLlmAlertGenerator(
    HttpClient httpClient,
    IOptions<LlmSettings> options) : ILlmAlertGenerator
{
    private readonly LlmSettings _settings = options.Value;
    private readonly HttpClient _httpClient = httpClient;

    public async Task<GeneratedAlert> GenerateAlert(NotificationRequest notification, CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, string.Empty)
        {
            Content = JsonContent.Create(new
            {
                model = _settings.Model,
                temperature = 0.2,
                messages = new[] 
                {
                    new
                    {
                        role = "system",
                        content = """
                            You turn operational alerts into concise Discord messages. 
                            Identify the likely category, impact, and recommended next step. 
                            Do not invent facts. Return plain text under 1,500 characters.
                        """
                    },
                    new
                    {
                        role = "user",
                        content = $"""
                            Level: {notification.Level}
                            Title: {notification.Title}
                            Source: {notification.Source ?? "unknown"}
                            Occurred at: {notification.Timestamp?.ToString("O") ?? "unknown"}
                            Details: {notification.Message}
                        """
                    }
                }
            })
        };

        var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStreamAsync(cancellationToken);
        var document = await JsonDocument.ParseAsync(content, cancellationToken: cancellationToken);
        var message = document
                        .RootElement
                        .GetProperty("choices")[0]
                        .GetProperty("message")
                        .GetProperty("content")
                        .GetString();
        if (string.IsNullOrWhiteSpace(message))
        { 
            throw new HttpRequestException("The LLM returned an empty alert message.");
        }

        return new GeneratedAlert(message[..Math.Min(message.Length, 1500)]);
    }
}
