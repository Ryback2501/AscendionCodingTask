using System.Net;
using System.Text;
using System.Text.Json;

namespace BestStories.Api.IntegrationTests;

/// <summary>
/// A fake Hacker News for the integration tests. The API calls it in place of the real one.
/// Tests add stories here, or make it answer with errors.
/// </summary>
public sealed class FakeHackerNews
{
    private readonly object _lock = new();
    private readonly List<int> _bestStoryIds = [];
    private readonly Dictionary<int, object> _items = [];
    private int _requestCount;

    /// <summary>When true, every call gets an HTTP 500 error.</summary>
    public bool IsDown { get; set; }

    /// <summary>How many calls the API made to this fake Hacker News.</summary>
    public int RequestCount => Volatile.Read(ref _requestCount);

    /// <summary>Adds a story to the list of best stories.</summary>
    public void AddStory(int id, int score, string title, string? url, string by, long time, int? descendants)
    {
        lock (_lock)
        {
            _bestStoryIds.Add(id);
            _items[id] = new { id, type = "story", score, title, url, by, time, descendants };
        }
    }

    /// <summary>Makes a new handler. The HTTP client factory calls this, and may throw old handlers away.</summary>
    public HttpMessageHandler CreateHandler() => new Handler(this);

    private HttpResponseMessage Answer(Uri uri)
    {
        Interlocked.Increment(ref _requestCount);
        if (IsDown)
        {
            return new HttpResponseMessage(HttpStatusCode.InternalServerError);
        }

        var path = uri.AbsolutePath;
        object? body;
        lock (_lock)
        {
            if (path.EndsWith("/beststories.json", StringComparison.Ordinal))
            {
                body = _bestStoryIds.ToArray();
            }
            else
            {
                var id = int.Parse(path.Split('/')[^1].Replace(".json", "", StringComparison.Ordinal),
                    System.Globalization.CultureInfo.InvariantCulture);
                body = _items.GetValueOrDefault(id);
            }
        }

        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json"),
        };
    }

    private sealed class Handler(FakeHackerNews hackerNews) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(hackerNews.Answer(request.RequestUri!));
    }
}
