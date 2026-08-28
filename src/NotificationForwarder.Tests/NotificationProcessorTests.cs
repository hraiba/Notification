using FakeItEasy;
using NotificationForwarder.Application;
using NotificationForwarder.Application.Contracts;
using NotificationForwarder.Application.Models;

namespace NotificationForwarder.Tests;

public sealed class NotificationProcessorTests
{
    [Fact]
    public async Task ProcessAsync_Info_ReturnsInformationalWithoutCallingDependencies()
    {
        var rateLimiter = A.Fake<IOutboundRateLimiter>();
        var generator = A.Fake<ILlmAlertGenerator>();
        var notifier = A.Fake<IDiscordNotifier>();
        var processor = new NotificationProcessor(notifier, generator, rateLimiter);

        var result = await processor.Process(new NotificationRequest(Title: "Deployment", Message: "Completed", Level:"info"), CancellationToken.None);

        Assert.Equal(NotificationProcessingOutcome.Informational, result.Outcome);
        Assert.False(result.Forwarded);
        A.CallTo(() => rateLimiter.TryAcquire()).MustNotHaveHappened();
        A.CallTo(() => generator.GenerateAlert(A<NotificationRequest>._, A<CancellationToken>._)).MustNotHaveHappened();
        A.CallTo(() => notifier.NotifyAsync(A<string>._, A<CancellationToken>._)).MustNotHaveHappened();
    }

    [Theory]
    [InlineData("warning")]
    [InlineData("error")]
    [InlineData("critical")]
    public async Task ProcessAsync_EscalationLevel_GeneratesAndForwardsAlert(string level)
    {
        var rateLimiter = A.Fake<IOutboundRateLimiter>();
        var generator = A.Fake<ILlmAlertGenerator>();
        var notifier = A.Fake<IDiscordNotifier>();
        using var cancellationSource = new CancellationTokenSource();
        var cancellationToken = cancellationSource.Token;
        var notification = new NotificationRequest( Title: "Disk space", Message: "Only 5% remains", Level: level, Source: "database-01");
        A.CallTo(() => rateLimiter.TryAcquire()).Returns(true);
        A.CallTo(() => generator.GenerateAlert(notification, cancellationToken)).Returns(Task.FromResult(new GeneratedAlert("Free disk space immediately.")));
        A.CallTo(() => notifier.NotifyAsync("Free disk space immediately.", cancellationToken)).Returns(Task.CompletedTask);
        var processor = new NotificationProcessor( notifier, generator, rateLimiter);

        var result = await processor.Process(notification, cancellationToken);

        Assert.Equal(NotificationProcessingOutcome.Forwarded, result.Outcome);
        Assert.True(result.Forwarded);
        A.CallTo(() => rateLimiter.TryAcquire()).MustHaveHappenedOnceExactly();
        A.CallTo(() => generator.GenerateAlert(notification, cancellationToken)).MustHaveHappenedOnceExactly();
        A.CallTo(() => notifier.NotifyAsync("Free disk space immediately.", cancellationToken)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ProcessAsync_WhenRateLimited_DoesNotCallExternalDependencies()
    {
        var rateLimiter = A.Fake<IOutboundRateLimiter>();
        var generator = A.Fake<ILlmAlertGenerator>();
        var notifier = A.Fake<IDiscordNotifier>();
        A.CallTo(() => rateLimiter.TryAcquire()).Returns(false);
        var processor = new NotificationProcessor(notifier, generator, rateLimiter);

        var result = await processor.Process(new NotificationRequest("Disk space", "Only 5% remains", "warning"), CancellationToken.None);

        Assert.Equal(NotificationProcessingOutcome.RateLimited, result.Outcome);
        Assert.False(result.Forwarded);
        A.CallTo(() => generator.GenerateAlert(A<NotificationRequest>._, A<CancellationToken>._)).MustNotHaveHappened();
        A.CallTo(() => notifier.NotifyAsync(A<string>._, A<CancellationToken>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task ProcessAsync_InvalidLevel_ReturnsInvalidWithoutConsumingRateLimit()
    {
        var rateLimiter = A.Fake<IOutboundRateLimiter>();
        var generator = A.Fake<ILlmAlertGenerator>();
        var notifier = A.Fake<IDiscordNotifier>();
        var processor = new NotificationProcessor(notifier,  generator, rateLimiter);

        var result = await processor.Process(new NotificationRequest("Deployment", "Completed", "verbose"), CancellationToken.None);

        Assert.Equal(NotificationProcessingOutcome.InvalidLevel, result.Outcome);
        Assert.False(result.Forwarded);
        A.CallTo(() => rateLimiter.TryAcquire()).MustNotHaveHappened();
        A.CallTo(() => generator.GenerateAlert(A<NotificationRequest>._, A<CancellationToken>._)).MustNotHaveHappened();
        A.CallTo(() => notifier.NotifyAsync(A<string>._, A<CancellationToken>._)).MustNotHaveHappened();
    }
}
