using System.Net;
using FakeItEasy;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NotificationForwarder.Application.Contracts;
using NotificationForwarder.Application.Models;

namespace NotificationForwarder.Tests.Integration;

public sealed class NotificationEndpointTests(NotificationApplicationFactory factory) : IClassFixture<NotificationApplicationFactory>
{
    private readonly HttpClient client = factory.CreateClient();
    private readonly NotificationApplicationFactory factory = factory;

    [Fact]
    public async Task PostInfo_DoesNotCallExternalServices()
    {
        factory.Reset();
        var response = await client.PostAsJsonAsync("/notifications", new NotificationRequest("Deployment", "Completed", "info"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        A.CallTo(() => factory.Notifier.Notify(A<string>._, A<CancellationToken>._)).MustNotHaveHappened();
        A.CallTo(() => factory.Generator.GenerateAlert(A<NotificationRequest>._, A<CancellationToken>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task PostWarning_GeneratesAndForwardsAlert()
    {
        factory.Reset();
        var response = await client.PostAsJsonAsync("/notifications", new NotificationRequest( "Disk space", "Only 5% remains", "warning", "database"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        A.CallTo(() => factory.Generator.GenerateAlert(A<NotificationRequest>.That.Matches(notification => notification.Title == "Disk space"), A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => factory.Notifier.Notify("Investigate: Disk space", A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task PostUnknownLevel_ReturnsBadRequest()
    {
        factory.Reset();
        var response = await client.PostAsJsonAsync("/notifications", new NotificationRequest("verbose", "Message", "Details"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}

public sealed class NotificationApplicationFactory : WebApplicationFactory<Program>
{
    public ILlmAlertGenerator Generator { get; } = A.Fake<ILlmAlertGenerator>();
    public IDiscordNotifier Notifier { get; } = A.Fake<IDiscordNotifier>();
    public IOutboundRateLimiter RateLimiter { get; } = A.Fake<IOutboundRateLimiter>();

    public NotificationApplicationFactory()
    {
        A.CallTo(() => Generator.GenerateAlert(A<NotificationRequest>._, A<CancellationToken>._)).Returns(Task.FromResult(new GeneratedAlert("Investigate: Disk space")));
        A.CallTo(() => Notifier.Notify(A<string>._, A<CancellationToken>._)).Returns(Task.CompletedTask);
        A.CallTo(() => RateLimiter.TryAcquire()).Returns(true);
    }

    public void Reset()
    {
        Fake.ClearRecordedCalls(Generator);
        Fake.ClearRecordedCalls(Notifier);
        Fake.ClearRecordedCalls(RateLimiter);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<ILlmAlertGenerator>();
            services.RemoveAll<IDiscordNotifier>();
            services.RemoveAll<IOutboundRateLimiter>();
            services.AddSingleton(Generator);
            services.AddSingleton(Notifier);
            services.AddSingleton(RateLimiter);
        });
    }
}
