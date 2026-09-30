using System.ComponentModel;
using Microsoft.AspNetCore.Http.HttpResults;

namespace BestStories.Api.Stories;

public static class BestStoriesEndpoints
{
    /// <summary>The highest allowed value for n. Hacker News gives about 200 best stories.</summary>
    public const int MaxStories = 200;

    /// <summary>Adds GET /api/stories/best to the app.</summary>
    public static IEndpointRouteBuilder MapBestStoriesEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/stories/best", GetBestStoriesAsync)
            .WithName("GetBestStories")
            .WithSummary("Gets the best n stories from Hacker News.")
            .WithDescription("Returns the best n stories from Hacker News, sorted by score (highest first).")
            .ProducesProblem(StatusCodes.Status503ServiceUnavailable);

        return app;
    }

    private static async Task<Results<Ok<IReadOnlyList<BestStory>>, ValidationProblem>> GetBestStoriesAsync(
        [Description("How many stories to return. A whole number from 1 to 200.")] int? n,
        IBestStoriesService service,
        CancellationToken cancellationToken)
    {
        if (n is null or < 1 or > MaxStories)
        {
            return TypedResults.ValidationProblem(
                new Dictionary<string, string[]>
                {
                    ["n"] = [$"n is required. It must be a whole number from 1 to {MaxStories}."],
                },
                title: "The value of n is not valid.");
        }

        var stories = await service.GetBestStoriesAsync(n.Value, cancellationToken);
        return TypedResults.Ok(stories);
    }
}
