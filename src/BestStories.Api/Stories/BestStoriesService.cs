using System.Globalization;
using BestStories.Api.HackerNews;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Options;

namespace BestStories.Api.Stories;

/// <summary>
/// Finds the best stories by score.
/// Hacker News gives a list of "best" story IDs, but that list is not sorted by score.
/// So we load every story in the list, and then we sort them by score ourselves.
/// Each story stays in a cache (a short-term memory) for a while, so we do not ask
/// Hacker News for the same story again and again.
/// </summary>
public sealed class BestStoriesService(
    IHackerNewsClient hackerNews,
    HybridCache cache,
    IOptions<HackerNewsOptions> options) : IBestStoriesService
{
    public async Task<IReadOnlyList<BestStory>> GetBestStoriesAsync(int count, CancellationToken cancellationToken)
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
        return items
            .Where(IsRealStory)
            .OrderByDescending(item => item!.Score)
            .Take(count)
            .Select(item => ToBestStory(item!))
            .ToList();
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
