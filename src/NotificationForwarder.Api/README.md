# Notification Forwarder API

This API accepts notification payloads, classifies them, optionally enriches them with a local AI-generated alert, and forwards the result to Discord when configured.

## Overview

The application is built with ASP.NET Core and uses:

- a local OpenAI-compatible AI endpoint for alert generation
- Discord webhook delivery for outbound notifications
- a simple in-memory outbound rate limiter
- an endpoint-based API for submitting notification events

## Configuration

The API reads LLM and Discord settings from configuration files such as:

- src/NotificationForwarder.Api/appsettings.Local.json
- src/NotificationForwarder.Api/appsettings.Development.json

Example:

```json
{
  "Discord": {
    "WebhookUrl": "https://discord.com/api/webhooks/..."
  },
  "LLM": {
    "Endpoint": "http://localhost:11434/v1/chat/completions",
    "ApiKey": "",
    "Model": "gemma4:e2b"
  }
}
```

## Run the API

From the repository root:

```bash
dotnet restore

dotnet run --project src/NotificationForwarder.Api/NotificationForwarder.Api.csproj
```

The API runs with the `Local` environment by default in launch settings and exposes Swagger/Scalar docs locally.

## Endpoint

### POST /notifications

Send a notification payload like this:

```json
{
  "title": "Database CPU spike",
  "message": "CPU usage exceeded 90% for 5 minutes on postgres-prod-01.",
  "level": "Warning",
  "source": "postgres-prod-01",
  "timestamp": "2026-08-29T12:00:00Z"
}
```

The service will:

1. validate the payload
2. generate a concise AI alert using the local model
3. apply rate limiting rules
4. forward the message to Discord if the request is allowed

## Health check

```bash
curl http://localhost:5229/health
```

## Troubleshooting

- If the app cannot reach the AI model, verify the `LLM:Endpoint` value.
- If the model is not available, pull it first in your local runner.
- If the Discord webhook fails, verify the `Discord:WebhookUrl` value.
- If your environment is configured for a hosted model instead, update the endpoint and key as needed.

## Notes

This repository intentionally favors a local-first AI setup so the API can run without an external OpenAI key or paid model access.
