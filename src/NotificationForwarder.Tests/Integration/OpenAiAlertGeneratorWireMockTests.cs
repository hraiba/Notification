using System.Text;
using Microsoft.Extensions.Options;
using NotificationForwarder.Application.Models;
using NotificationForwarder.Infrastructure;
using NotificationForwarder.Infrastructure.Settings;
using WireMock.Net.Testcontainers;

namespace NotificationForwarder.Tests.Integration;

public sealed class OpenAiAlertGeneratorWireMockTests
{
    [Fact]
    public async Task GenerateAsync_UsesOpenAiCompatibleEndpointAndReturnsGeneratedMessage()
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

        using var adminClient = wireMock.CreateClient();
        using var mappingContent = new StringContent(
            """
                {
                  "Request": {
                    "Path": { "Matchers": [{ "Name": "WildcardMatcher", "Pattern": "/v1/responses" }] },
                    "Methods": ["post"]
                  },
                  "Response": {
                    "StatusCode": 200,
                    "Body": "{ \"output\": [{ \"type\": \"message\", \"content\": [{ \"type\": \"output_text\", \"text\": \"Storage warning: free space is low. Clean up old backups.\" }] }] }",
                    "Headers": { "Content-Type": "application/json" }
                  }
                }
                """,
            Encoding.UTF8,
            "application/json");
        var mappingResponse = await adminClient.PostAsync("/__admin/mappings", mappingContent);
        mappingResponse.EnsureSuccessStatusCode();

        using var llmHttpClient = wireMock.CreateClient();
        llmHttpClient.BaseAddress = new Uri(new Uri(wireMock.GetPublicUrl()), "v1/responses");
        var options = Options.Create(new LlmSettings { Model = "llama3.2" });
        var generator = new OpenAiLlmAlertGenerator(llmHttpClient, options);

        var result = await generator.GenerateAlert(
            new NotificationRequest("warning", "Disk space", "Only 5% remains", "database-01"),
            CancellationToken.None);

        Assert.Equal("Storage warning: free space is low. Clean up old backups.", result.Message);
    }
}

