using System;
using NotificationForwarder.Application.Contracts;
using NotificationForwarder.Application.Models;

namespace NotificationForwarder.Infrastructure;

public class OpenAiLlmAlertGenerator : ILLMAlertGenerator
{
    public Task<GeneratedAlert> GenerateAlert(NotificationRequest request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new GeneratedAlert($"[LLM Alert] {request.Level}: {request.Message}"));
    }
}
