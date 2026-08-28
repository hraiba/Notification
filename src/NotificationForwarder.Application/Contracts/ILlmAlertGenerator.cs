using NotificationForwarder.Application.Models;

namespace NotificationForwarder.Application.Contracts;

public interface ILlmAlertGenerator
{
    Task<GeneratedAlert> GenerateAlert(NotificationRequest request, CancellationToken cancellationToken = default);
}
