namespace NotificationForwarder.Application.Models;

public sealed record NotificationRequest(
    string Title,
    string Message,
    string Level,
    string? Source = null,
    DateTimeOffset? Timestamp = null
);