using Microsoft.Extensions.Options;
using NotificationForwarder.Api.Endpoints;
using NotificationForwarder.Application.Contracts;
using NotificationForwarder.Infrastructure;
using NotificationForwarder.Infrastructure.Settings;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.Configure<DiscordSettings>(builder.Configuration.GetSection("Discord"));
builder.Services.Configure<LlmSettings>(builder.Configuration.GetSection("LLM"));

builder.Services.AddOpenApi();
builder.Services.AddTransient<NotificationProcessor>();
builder.Services.AddHttpClient<IDiscordNotifier, DiscordNotifier>(
    client => client.Timeout = TimeSpan.FromSeconds(30)
);
builder.Services.AddHttpClient<ILlmAlertGenerator, OpenAiLlmAlertGenerator>(
    (sp, client) =>
    {
        var settings = sp.GetRequiredService<IOptions<LlmSettings>>().Value;
        client.BaseAddress = new Uri(settings.Endpoint);
        client.Timeout = TimeSpan.FromSeconds(120);
    }
);

builder.Services.AddSingleton<IOutboundRateLimiter, OutboundRateLimiter>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.EnvironmentName == "Local")
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapNotificationEndpoints();
app.MapGet("/health", () => Results.Ok("Healthy"));

app.UseHttpsRedirection();
app.Run();

