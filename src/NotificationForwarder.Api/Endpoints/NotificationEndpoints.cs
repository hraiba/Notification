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
        try
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
        catch(HttpRequestException ex)
        {
            return Results.Problem($"An error occurred while forwarding the notification: {ex.Message}", statusCode: StatusCodes.Status502BadGateway);
        }
        catch(TaskCanceledException)
        {
            return Results.Problem("The request was canceled due to a timeout.", statusCode: StatusCodes.Status504GatewayTimeout);
        }
        catch(InvalidOperationException ex)
        {
            return Results.Problem($"Invalid operation: {ex.Message}", statusCode: StatusCodes.Status400BadRequest);
        }
        catch(ArgumentException ex)
        {
            return Results.Problem($"Invalid argument: {ex.Message}", statusCode: StatusCodes.Status400BadRequest);
        }
        catch (Exception ex)
        {
            return Results.Problem($"An error occurred while processing the notification: {ex.Message}", statusCode: StatusCodes.Status500InternalServerError);
        }
    }
}