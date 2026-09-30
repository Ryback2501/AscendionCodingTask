using System.ComponentModel.DataAnnotations;

namespace BestStories.Api.HackerNews;

/// <summary>
/// Settings for the calls to the Hacker News API.
/// They come from the "HackerNews" section of appsettings.json.
/// </summary>
public sealed class HackerNewsOptions
{
    public const string SectionName = "HackerNews";

    /// <summary>The address of the Hacker News API. It must end with "/".</summary>
    [Required]
    [Url]
    public string BaseUrl { get; set; } = "https://hacker-news.firebaseio.com/v0/";
}
