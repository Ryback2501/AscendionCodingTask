using System.Globalization;
using System.Text.Json;
using Polly.CircuitBreaker;
using Polly.Timeout;

namespace BestStories.Api.HackerNews;

/// <summary>
/// Calls the Hacker News API over HTTP.
/// The HttpClient already has the Hacker News address (see AddHackerNewsClient).
/// If we cannot get good data from Hacker News, the methods throw a HackerNewsUnavailableException.
/// </summary>
public sealed class HackerNewsClient(HttpClient httpClient) : IHackerNewsClient
{
    public async Task<IReadOnlyList<int>> GetBestStoryIdsAsync(CancellationToken cancellationToken)
    {
        var ids = await GetAsync<int[]>("beststories.json", cancellationToken);
        return ids ?? [];
    }

    public Task<HackerNewsItem?> GetItemAsync(int id, CancellationToken cancellationToken) =>
        GetAsync<HackerNewsItem>(string.Create(CultureInfo.InvariantCulture, $"item/{id}.json"), cancellationToken);

    private async Task<T?> GetAsync<T>(string path, CancellationToken cancellationToken)
    {
        try
        {
            return await httpClient.GetFromJsonAsync<T>(path, cancellationToken);
        }
        catch (Exception error) when (error is HttpRequestException
                                          or JsonException
                                          or TimeoutRejectedException
                                          or BrokenCircuitException)
        {
            throw new HackerNewsUnavailableException($"Could not read '{path}' from Hacker News.", error);
        }
    }
}
