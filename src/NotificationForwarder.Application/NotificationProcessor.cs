using NotificationForwarder.Application.Contracts;
using NotificationForwarder.Application.Models;

public sealed class NotificationProcessor(
    IDiscordNotifier discordNotifier,
    ILLMAlertGenerator llmAlertGenerator)
{
    private readonly IDiscordNotifier _discordNotifier = discordNotifier;
    private readonly ILLMAlertGenerator _llmAlertGenerator = llmAlertGenerator;

    public async Task<NotificationResult> Process(
        NotificationRequest request,
        CancellationToken cancellationToken)
    {
        return Enum.TryParse<NotificationLevel>(request.Level, true, out var level)
            ? level switch
            {
                NotificationLevel.Info => new NotificationResult(
                    false,
                    "Informational notifications are not forwarded.",
                    NotificationProcessingOutcome.Informational),
                NotificationLevel.Warning or
                NotificationLevel.Error or
                NotificationLevel.Critical => await Forward(request, cancellationToken),
                _ => new NotificationResult(
                    false,
                    $"Invalid notification level: {request.Level}",
                    NotificationProcessingOutcome.InvalidLevel)
            }
            : new NotificationResult(
                false,
                $"Level must be on of: {string.Join(", ", Enum.GetNames<NotificationLevel>())}",
                NotificationProcessingOutcome.InvalidLevel);
    }
    private async Task<NotificationResult> Forward(NotificationRequest request, CancellationToken cancellationToken)
    {
        var alert = await _llmAlertGenerator.GenerateAlert(request, cancellationToken);
        await _discordNotifier.NotifyAsync(alert.Message, cancellationToken);
        return new NotificationResult(
            true,
            "Notification forwarded successfully.",
            NotificationProcessingOutcome.Forwarded);
    }
}