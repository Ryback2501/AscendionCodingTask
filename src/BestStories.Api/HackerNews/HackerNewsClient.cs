using System.Globalization;

namespace BestStories.Api.HackerNews;

/// <summary>
/// Calls the Hacker News API over HTTP.
/// The HttpClient already has the Hacker News address (see AddHackerNewsClient).
/// If Hacker News answers with an error, the methods throw an HttpRequestException.
/// </summary>
public sealed class HackerNewsClient(HttpClient httpClient) : IHackerNewsClient
{
    public async Task<IReadOnlyList<int>> GetBestStoryIdsAsync(CancellationToken cancellationToken)
    {
        var ids = await httpClient.GetFromJsonAsync<int[]>("beststories.json", cancellationToken);
        return ids ?? [];
    }

    public Task<HackerNewsItem?> GetItemAsync(int id, CancellationToken cancellationToken) =>
        httpClient.GetFromJsonAsync<HackerNewsItem>(
            string.Create(CultureInfo.InvariantCulture, $"item/{id}.json"), cancellationToken);
}
