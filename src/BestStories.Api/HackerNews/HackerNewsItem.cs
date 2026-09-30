namespace BestStories.Api.HackerNews;

/// <summary>
/// One item from the Hacker News API, for example a story.
/// Hacker News can leave out any field, so most fields can be empty (null).
/// </summary>
public sealed record HackerNewsItem
{
    public int Id { get; init; }

    /// <summary>The kind of item: "story", "comment", "job", "poll" or "pollopt".</summary>
    public string? Type { get; init; }

    /// <summary>The user name of the author.</summary>
    public string? By { get; init; }

    /// <summary>The time when the item was posted, in Unix time (seconds since 1 January 1970, UTC).</summary>
    public long Time { get; init; }

    public string? Title { get; init; }

    /// <summary>The web address of the story. Some stories (like "Ask HN") have no address.</summary>
    public string? Url { get; init; }

    public int Score { get; init; }

    /// <summary>The total number of comments.</summary>
    public int? Descendants { get; init; }

    /// <summary>True when the item was deleted.</summary>
    public bool Deleted { get; init; }

    /// <summary>True when the item was hidden by Hacker News.</summary>
    public bool Dead { get; init; }
}
