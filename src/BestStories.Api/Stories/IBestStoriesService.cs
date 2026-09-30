namespace BestStories.Api.Stories;

/// <summary>Finds the best stories on Hacker News.</summary>
public interface IBestStoriesService
{
    /// <summary>
    /// Gets the best stories, sorted by score (highest first).
    /// It returns at most <paramref name="count"/> stories.
    /// </summary>
    Task<IReadOnlyList<BestStory>> GetBestStoriesAsync(int count, CancellationToken cancellationToken);
}
