using BestStories.Api.HackerNews;

namespace BestStories.Api.UnitTests.Stories;

/// <summary>
/// A fake Hacker News client for tests. It gives the IDs and items that the test sets.
/// It also counts how many item calls run at the same time.
/// </summary>
public sealed class FakeHackerNewsClient : IHackerNewsClient
{
    private readonly object _lock = new();
    private int _callsRunningNow;

    public List<int> BestStoryIds { get; } = [];

    public Dictionary<int, HackerNewsItem?> Items { get; } = [];

    /// <summary>How long each item call waits before it answers.</summary>
    public TimeSpan ItemDelay { get; set; } = TimeSpan.Zero;

    /// <summary>When set, every item call throws this error.</summary>
    public Exception? ItemError { get; set; }

    /// <summary>The highest number of item calls that ran at the same time.</summary>
    public int MostCallsAtTheSameTime { get; private set; }

    /// <summary>Adds a normal story to the list of best stories.</summary>
    public void AddStory(int id, int score, string? type = "story")
    {
        BestStoryIds.Add(id);
        Items[id] = new HackerNewsItem { Id = id, Type = type, Title = $"Story {id}", Score = score };
    }

    public Task<IReadOnlyList<int>> GetBestStoryIdsAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<int>>(BestStoryIds);

    public async Task<HackerNewsItem?> GetItemAsync(int id, CancellationToken cancellationToken)
    {
        lock (_lock)
        {
            _callsRunningNow++;
            MostCallsAtTheSameTime = Math.Max(MostCallsAtTheSameTime, _callsRunningNow);
        }

        try
        {
            await Task.Delay(ItemDelay, cancellationToken);
            if (ItemError is not null)
            {
                throw ItemError;
            }

            return Items.GetValueOrDefault(id);
        }
        finally
        {
            lock (_lock)
            {
                _callsRunningNow--;
            }
        }
    }
}
