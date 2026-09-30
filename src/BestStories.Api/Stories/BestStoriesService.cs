using BestStories.Api.HackerNews;
using Microsoft.Extensions.Options;

namespace BestStories.Api.Stories;

/// <summary>
/// Finds the best stories by score.
/// Hacker News gives a list of "best" story IDs, but that list is not sorted by score.
/// So we load every story in the list, and then we sort them by score ourselves.
/// </summary>
public sealed class BestStoriesService(IHackerNewsClient hackerNews, IOptions<HackerNewsOptions> options)
    : IBestStoriesService
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
            items[index] = await hackerNews.GetItemAsync(ids[index], token);
        });

        // OrderByDescending keeps the Hacker News order for stories with the same score.
        return items
            .Where(IsRealStory)
            .OrderByDescending(item => item!.Score)
            .Take(count)
            .Select(item => ToBestStory(item!))
            .ToList();
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
