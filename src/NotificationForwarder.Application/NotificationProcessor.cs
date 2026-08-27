using NotificationForwarder.Application.Models;

public sealed class NotificationProcessor
{
    public async Task<NotificationResult> Process(
        NotificationRequest request,
        CancellationToken cancellationToken)
    {
        // Simulate some processing logic
        await Task.Delay(100, cancellationToken); // Simulate async work

        // For demonstration, we assume the notification is always forwarded successfully
        return new NotificationResult(
            true,
            "Notification processed successfully",
            NotificationProcessingOutcome.Forwarded);
    }
}