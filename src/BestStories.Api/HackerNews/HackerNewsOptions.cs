using System.ComponentModel.DataAnnotations;

namespace BestStories.Api.HackerNews;

/// <summary>
/// Settings for the calls to the Hacker News API.
/// They come from the "HackerNews" section of appsettings.json.
/// </summary>
public sealed class HackerNewsOptions
{
    public const string SectionName = "HackerNews";

    /// <summary>The address of the Hacker News API.</summary>
    [Required]
    [Url]
    public string BaseUrl { get; set; } = "https://hacker-news.firebaseio.com/v0/";

    /// <summary>
    /// The highest number of calls to Hacker News at the same time, when we load many stories.
    /// A small number protects Hacker News from too many calls.
    /// </summary>
    [Range(1, 50)]
    public int MaxParallelRequests { get; set; } = 8;

    /// <summary>
    /// How long (in seconds) we keep each story in the cache (a short-term memory).
    /// During this time, we do not ask Hacker News for this story again.
    /// </summary>
    [Range(1, 86400)]
    public int StoryCacheSeconds { get; set; } = 300;
}
