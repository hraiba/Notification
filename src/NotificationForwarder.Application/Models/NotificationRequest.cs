using System.Text.Json.Serialization;

namespace NotificationForwarder.Application.Models;

public sealed record NotificationRequest(
   [property:JsonPropertyName("title")] string Title,
   [property:JsonPropertyName("message")] string Message,
   [property:JsonPropertyName("level")] string Level,
   [property:JsonPropertyName("source")] string? Source = null,
   [property:JsonPropertyName("timestamp")] DateTimeOffset? Timestamp = null);