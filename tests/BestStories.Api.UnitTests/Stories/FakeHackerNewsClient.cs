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
    private int _idCallCount;
    private int _itemCallCount;

    public List<int> BestStoryIds { get; } = [];

    public Dictionary<int, HackerNewsItem?> Items { get; } = [];

    /// <summary>How long each item call waits before it answers.</summary>
    public TimeSpan ItemDelay { get; set; } = TimeSpan.Zero;

    /// <summary>When set, every item call throws this error.</summary>
    public Exception? ItemError { get; set; }

    /// <summary>How many times the list of best story IDs was asked for.</summary>
    public int IdCallCount => Volatile.Read(ref _idCallCount);

    /// <summary>How many times an item was asked for (all IDs together).</summary>
    public int ItemCallCount => Volatile.Read(ref _itemCallCount);

    /// <summary>The highest number of item calls that ran at the same time.</summary>
    public int MostCallsAtTheSameTime { get; private set; }

    /// <summary>Adds a normal story to the list of best stories.</summary>
    public void AddStory(int id, int score, string? type = "story")
    {
        BestStoryIds.Add(id);
        Items[id] = new HackerNewsItem { Id = id, Type = type, Title = $"Story {id}", Score = score };
    }

    public Task<IReadOnlyList<int>> GetBestStoryIdsAsync(CancellationToken cancellationToken)
    {
        Interlocked.Increment(ref _idCallCount);
        return Task.FromResult<IReadOnlyList<int>>([.. BestStoryIds]);
    }

    public async Task<HackerNewsItem?> GetItemAsync(int id, CancellationToken cancellationToken)
    {
        Interlocked.Increment(ref _itemCallCount);
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
