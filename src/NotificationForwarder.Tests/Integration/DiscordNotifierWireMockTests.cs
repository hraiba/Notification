using System.Text;
using Microsoft.Extensions.Options;
using NotificationForwarder.Infrastructure;
using NotificationForwarder.Infrastructure.Settings;
using WireMock.Net.Testcontainers;

namespace NotificationForwarder.Tests.Integration;

public sealed class DiscordNotifierWireMockTests
{
    [Fact]
    public async Task SendAsync_PostsToConfiguredWebhook()
    {
            var containerBuilder = new WireMockContainerBuilder()
                .WithAutoRemove(true)
                .WithCleanUp(true);

            var containerRuntimeEndpoint = Environment.GetEnvironmentVariable("DOCKER_HOST");
            if (!string.IsNullOrWhiteSpace(containerRuntimeEndpoint))
            {
                containerBuilder = containerBuilder.WithDockerEndpoint(containerRuntimeEndpoint);
            }

            await using var wireMock = containerBuilder.Build();
            await wireMock.StartAsync();

            using var httpClient = wireMock.CreateClient();
            using var mappingContent = new StringContent(
                """
                {
                  "Request": {
                    "Path": { "Matchers": [{ "Name": "WildcardMatcher", "Pattern": "/discord/webhook" }] },
                    "Methods": ["post"]
                  },
                  "Response": { "StatusCode": 204 }
                }
                """,
                Encoding.UTF8,
                "application/json");
            var mappingResponse = await httpClient.PostAsync("/__admin/mappings", mappingContent);
            mappingResponse.EnsureSuccessStatusCode();

            var webhookUrl = new Uri(new Uri(wireMock.GetPublicUrl()), "discord/webhook").ToString();
            var options = Options.Create(new DiscordSettings { WebhookUrl = webhookUrl });
            var notifier = new DiscordNotifier(httpClient, options);

            await notifier.NotifyAsync("Database alert", CancellationToken.None);
    }
}
