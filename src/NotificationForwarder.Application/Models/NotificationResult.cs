namespace NotificationForwarder.Application.Models;

public sealed record NotificationResult(
    bool Forwarded,
    string Detail,
    NotificationProcessingOutcome Outcome);