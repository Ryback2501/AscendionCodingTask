namespace BestStories.Api.HackerNews;

/// <summary>Reads data from the Hacker News API.</summary>
public interface IHackerNewsClient
{
    /// <summary>
    /// Gets the IDs of the best stories (up to about 200), in the order that Hacker News gives.
    /// </summary>
    Task<IReadOnlyList<int>> GetBestStoryIdsAsync(CancellationToken cancellationToken);

    /// <summary>Gets one item. Returns null when Hacker News does not know the ID.</summary>
    Task<HackerNewsItem?> GetItemAsync(int id, CancellationToken cancellationToken);
}
