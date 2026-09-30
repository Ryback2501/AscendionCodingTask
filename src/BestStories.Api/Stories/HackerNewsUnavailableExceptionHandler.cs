using BestStories.Api.HackerNews;
using Microsoft.AspNetCore.Diagnostics;

namespace BestStories.Api.Stories;

/// <summary>
/// When we cannot get data from Hacker News, answer 503 (Service Unavailable)
/// with a clear message, not 500 (Internal Server Error).
/// </summary>
public sealed partial class HackerNewsUnavailableExceptionHandler(
    IProblemDetailsService problemDetails,
    ILogger<HackerNewsUnavailableExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not HackerNewsUnavailableException)
        {
            // Not our error: let the normal error handling answer 500.
            return false;
        }

        LogHackerNewsUnavailable(logger, exception);

        httpContext.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
        return await problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails =
            {
                Status = StatusCodes.Status503ServiceUnavailable,
                Title = "Hacker News is not available. Please try again later.",
            },
        });
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Could not get data from Hacker News.")]
    private static partial void LogHackerNewsUnavailable(ILogger logger, Exception exception);
}
