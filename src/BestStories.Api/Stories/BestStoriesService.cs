using System.Globalization;
using BestStories.Api.HackerNews;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Options;

namespace BestStories.Api.Stories;

/// <summary>
/// Finds the best stories by score.
/// Hacker News gives a list of "best" story IDs, but that list is not sorted by score.
/// So we load every story in the list, and then we sort them by score ourselves.
///
/// To protect Hacker News, we use a cache (a short-term memory) at two levels:
/// - the full sorted list of stories, for a short time (BestStoriesCacheSeconds),
/// - each story, for a longer time (StoryCacheSeconds).
/// So most requests do not call Hacker News at all.
/// </summary>
public sealed class BestStoriesService(
    IHackerNewsClient hackerNews,
    HybridCache cache,
    IOptions<HackerNewsOptions> options) : IBestStoriesService
{
    private const string BestStoriesCacheKey = "best-stories";

    public async Task<IReadOnlyList<BestStory>> GetBestStoriesAsync(int count, CancellationToken cancellationToken)
    {
        // When many requests arrive at the same time and the list is not in the cache,
        // only one of them builds the list. The other requests wait for it.
        var cacheTime = TimeSpan.FromSeconds(options.Value.BestStoriesCacheSeconds);
        var rankedStories = await cache.GetOrCreateAsync(
            BestStoriesCacheKey,
            this,
            static (service, token) => service.LoadRankedStoriesAsync(token),
            new HybridCacheEntryOptions { Expiration = cacheTime, LocalCacheExpiration = cacheTime },
            cancellationToken: cancellationToken);

        return rankedStories.Stories.Take(count).ToList();
    }

    /// <summary>Loads all the best stories and sorts them by score, highest first.</summary>
    private async ValueTask<RankedStories> LoadRankedStoriesAsync(CancellationToken cancellationToken)
    {
        var ids = await hackerNews.GetBestStoryIdsAsync(cancellationToken);
        var items = new HackerNewsItem?[ids.Count];

        // Load the stories at the same time, but never more than MaxParallelRequests at once.
        // Each story goes to the same position as its ID, so we keep the Hacker News order.
        var parallelOptions = new ParallelOptions
        {
            MaxDegreeOfParallelism = options.Value.MaxParallelRequests,
            CancellationToken = cancellationToken,
        };
        await Parallel.ForEachAsync(Enumerable.Range(0, ids.Count), parallelOptions, async (index, token) =>
        {
            items[index] = await GetItemAsync(ids[index], token);
        });

        // OrderByDescending keeps the Hacker News order for stories with the same score.
        var stories = items
            .Where(IsRealStory)
            .OrderByDescending(item => item!.Score)
            .Select(item => ToBestStory(item!))
            .ToList();

        return new RankedStories(stories);
    }

    /// <summary>
    /// Gets one story from the cache. If the cache does not have it, loads it from Hacker News.
    /// When many requests want the same story at the same time, only one of them calls Hacker News.
    /// Errors are not kept in the cache, so the next request tries again.
    /// </summary>
    private ValueTask<HackerNewsItem?> GetItemAsync(int id, CancellationToken cancellationToken)
    {
        var cacheTime = TimeSpan.FromSeconds(options.Value.StoryCacheSeconds);
        return cache.GetOrCreateAsync(
            string.Create(CultureInfo.InvariantCulture, $"hn-item:{id}"),
            (hackerNews, id),
            static async (state, token) => await state.hackerNews.GetItemAsync(state.id, token),
            new HybridCacheEntryOptions { Expiration = cacheTime, LocalCacheExpiration = cacheTime },
            cancellationToken: cancellationToken);
    }

    private static bool IsRealStory(HackerNewsItem? item) =>
        item is { Type: "story", Deleted: false, Dead: false };

    private static BestStory ToBestStory(HackerNewsItem item) => new(
        Title: item.Title ?? "",
        Uri: item.Url,
        PostedBy: item.By ?? "",
        Time: DateTimeOffset.FromUnixTimeSeconds(item.Time),
        Score: item.Score,
        CommentCount: item.Descendants ?? 0);
}
