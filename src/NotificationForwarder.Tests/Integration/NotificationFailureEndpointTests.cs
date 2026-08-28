using System.Net;
using FakeItEasy;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NotificationForwarder.Application.Contracts;
using NotificationForwarder.Application.Models;

namespace NotificationForwarder.Tests.Integration;

public sealed class NotificationFailureEndpointTests
{
    [Fact]
    public async Task PostWarning_WhenOutboundRateLimitIsReached_ReturnsTooManyRequests()
    {
        var rateLimiter = A.Fake<IOutboundRateLimiter>();
        A.CallTo(() => rateLimiter.TryAcquire()).Returns(false);
        using var factory = new ConfiguredNotificationApplicationFactory(SuccessfulNotifier(), SuccessfulAlertGenerator(), rateLimiter);
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/notifications", WarningNotification());

        Assert.Equal(HttpStatusCode.TooManyRequests, response.StatusCode);
    }

    [Fact]
    public async Task PostWarning_WhenLlmRequestFails_ReturnsBadGateway()
    {
        using var factory = new ConfiguredNotificationApplicationFactory(SuccessfulNotifier(), ThrowingAlertGenerator(new HttpRequestException()), AllowedRateLimiter());
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/notifications", WarningNotification());

        Assert.Equal(HttpStatusCode.BadGateway, response.StatusCode);
    }

    [Fact]
    public async Task PostWarning_WhenDiscordRequestFails_ReturnsBadGateway()
    {
        using var factory = new ConfiguredNotificationApplicationFactory(ThrowingNotifier(new HttpRequestException()), SuccessfulAlertGenerator(), AllowedRateLimiter());
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/notifications", WarningNotification());

        Assert.Equal(HttpStatusCode.BadGateway, response.StatusCode);
    }

    private static NotificationRequest WarningNotification() => new("Disk space", "Only 5% remains", "warning", "database");

    private static IOutboundRateLimiter AllowedRateLimiter()
    {
        var rateLimiter = A.Fake<IOutboundRateLimiter>();
        A.CallTo(() => rateLimiter.TryAcquire()).Returns(true);
        return rateLimiter;
    }

    private static ILlmAlertGenerator SuccessfulAlertGenerator()
    {
        var alertGenerator = A.Fake<ILlmAlertGenerator>();
        A.CallTo(() => alertGenerator.GenerateAlert(A<NotificationRequest>._, A<CancellationToken>._)).Returns(Task.FromResult(new GeneratedAlert("Generated alert")));
        return alertGenerator;
    }

    private static ILlmAlertGenerator ThrowingAlertGenerator(Exception exception)
    {
        var alertGenerator = A.Fake<ILlmAlertGenerator>();
        A.CallTo(() => alertGenerator.GenerateAlert(A<NotificationRequest>._, A<CancellationToken>._)).Returns(Task.FromException<GeneratedAlert>(exception));
        return alertGenerator;
    }

    private static IDiscordNotifier SuccessfulNotifier()
    {
        var notifier = A.Fake<IDiscordNotifier>();
        A.CallTo(() => notifier.Notify(A<string>._, A<CancellationToken>._)).Returns(Task.CompletedTask);
        return notifier;
    }

    private static IDiscordNotifier ThrowingNotifier(Exception exception)
    {
        var notifier = A.Fake<IDiscordNotifier>();
        A.CallTo(() => notifier.Notify(A<string>._, A<CancellationToken>._)).Returns(Task.FromException(exception));
        return notifier;
    }
}

public sealed class ConfiguredNotificationApplicationFactory(
    IDiscordNotifier notifier,
    ILlmAlertGenerator alertGenerator,
    IOutboundRateLimiter rateLimiter) : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IDiscordNotifier>();
            services.RemoveAll<ILlmAlertGenerator>();
            services.RemoveAll<IOutboundRateLimiter>();
            services.AddSingleton(notifier);
            services.AddSingleton(alertGenerator);
            services.AddSingleton(rateLimiter);
        });
    }
}
