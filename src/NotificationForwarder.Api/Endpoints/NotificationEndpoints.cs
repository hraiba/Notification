using NotificationForwarder.Application.Models;

namespace NotificationForwarder.Api.Endpoints;

public static class NotificationEndpoints
{
    public static IEndpointRouteBuilder MapNotificationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/notifications", Handle);
        return endpoints;
    }


    private static async Task<IResult> Handle(
        NotificationRequest request,
        NotificationProcessor Processor,
        CancellationToken cancellationToken)
    {
        var result  = await Processor.Process(request, cancellationToken);
        return result.Outcome switch
        {
            NotificationProcessingOutcome.Informational => Results.Ok(result),
            NotificationProcessingOutcome.Forwarded => Results.Ok(result),
            NotificationProcessingOutcome.InvalidLevel => Results.BadRequest(result),
            NotificationProcessingOutcome.RateLimited => Results.Json(result, statusCode: StatusCodes.Status429TooManyRequests),
            _ => Results.Problem("Unknown outcome", statusCode: StatusCodes.Status500InternalServerError),
        };
    }
}