using System;
using NotificationForwarder.Application.Models;

namespace NotificationForwarder.Application.Contracts;

public interface ILLMAlertGenerator
{
    Task<GeneratedAlert> GenerateAlert(NotificationRequest request, CancellationToken cancellationToken = default);
}
