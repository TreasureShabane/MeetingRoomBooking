using MeetingRoomBooking.Application.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace MeetingRoomBooking.Api.Middleware;

public sealed class ApiExceptionHandler(ILogger<ApiExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        ProblemDetails problemDetails;
        switch (exception)
        {
            case BookingValidationException validationException:
                problemDetails = new ValidationProblemDetails(new Dictionary<string, string[]>
                {
                    ["booking"] = validationException.Errors.ToArray()
                })
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Booking validation failed",
                    Instance = httpContext.Request.Path
                };
                break;
            case BookingConflictException conflictException:
                problemDetails = new ProblemDetails
                {
                    Status = StatusCodes.Status409Conflict,
                    Title = "Booking conflict",
                    Detail = conflictException.Message,
                    Instance = httpContext.Request.Path
                };
                break;
            case KeyNotFoundException notFoundException:
                problemDetails = new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = "Resource not found",
                    Detail = notFoundException.Message,
                    Instance = httpContext.Request.Path
                };
                break;
            default:
                return false;
        }

        logger.LogWarning(exception, "Handled API error with status {StatusCode}", problemDetails.Status);
        httpContext.Response.StatusCode = problemDetails.Status!.Value;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        return true;
    }
}
