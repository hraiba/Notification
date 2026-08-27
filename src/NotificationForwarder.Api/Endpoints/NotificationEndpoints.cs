using NotificationForwarder.Application.Models;

namespace NotificationForwarder.Api.Endpoints;

public static class NotificationEndpoints
{
    public static IEndpointRouteBuilder MapNotificationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/notifications", Handle);
        return endpoints;
    }


    private static async Task<IResult> Handle()
    {
        // Handle the notification here
        await Task.Delay(100); // Simulate some async work
        return Results.Ok(new NotificationResult(
            true,
            "Notification processed successfully",
            NotificationProcessingOutcome.Forwarded));
    }
}