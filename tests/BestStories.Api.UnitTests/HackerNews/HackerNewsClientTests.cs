using System.Net;
using System.Text.Json;
using BestStories.Api.HackerNews;

namespace BestStories.Api.UnitTests.HackerNews;

public class HackerNewsClientTests
{
    private static readonly Uri _baseAddress = new("https://hn.test/v0/");

    private static (HackerNewsClient Client, FakeHttpMessageHandler Handler) CreateClient(
        Func<HttpRequestMessage, HttpResponseMessage> respond)
    {
        var handler = new FakeHttpMessageHandler(respond);
        var httpClient = new HttpClient(handler) { BaseAddress = _baseAddress };
        return (new HackerNewsClient(httpClient), handler);
    }

    [Fact]
    public async Task Gets_the_best_story_ids()
    {
        var (client, handler) = CreateClient(_ => FakeHttpMessageHandler.Json("[3, 1, 2]"));

        var ids = await client.GetBestStoryIdsAsync(CancellationToken.None);

        Assert.Equal([3, 1, 2], ids);
        Assert.Equal(new Uri("https://hn.test/v0/beststories.json"), Assert.Single(handler.RequestedUris));
    }

    [Fact]
    public async Task Gets_one_item()
    {
        var (client, handler) = CreateClient(_ => FakeHttpMessageHandler.Json(
            """{ "id": 21233041, "type": "story", "title": "A title", "score": 1757 }"""));

        var item = await client.GetItemAsync(21233041, CancellationToken.None);

        Assert.NotNull(item);
        Assert.Equal(21233041, item.Id);
        Assert.Equal("A title", item.Title);
        Assert.Equal(1757, item.Score);
        Assert.Equal(new Uri("https://hn.test/v0/item/21233041.json"), Assert.Single(handler.RequestedUris));
    }

    [Fact]
    public async Task Gives_no_item_when_hacker_news_answers_null()
    {
        var (client, _) = CreateClient(_ => FakeHttpMessageHandler.Json("null"));

        var item = await client.GetItemAsync(42, CancellationToken.None);

        Assert.Null(item);
    }

    [Fact]
    public async Task Gives_an_empty_list_when_hacker_news_answers_null_for_the_ids()
    {
        var (client, _) = CreateClient(_ => FakeHttpMessageHandler.Json("null"));

        var ids = await client.GetBestStoryIdsAsync(CancellationToken.None);

        Assert.Empty(ids);
    }

    [Fact]
    public async Task Throws_when_hacker_news_answers_with_an_error()
    {
        var (client, _) = CreateClient(_ =>
            FakeHttpMessageHandler.Json("{}", HttpStatusCode.InternalServerError));

        var error = await Assert.ThrowsAsync<HackerNewsUnavailableException>(
            () => client.GetItemAsync(1, CancellationToken.None));
        Assert.IsType<HttpRequestException>(error.InnerException);
        await Assert.ThrowsAsync<HackerNewsUnavailableException>(
            () => client.GetBestStoryIdsAsync(CancellationToken.None));
    }

    [Fact]
    public async Task Throws_when_hacker_news_answers_with_broken_json()
    {
        var (client, _) = CreateClient(_ => FakeHttpMessageHandler.Json("{ this is not json"));

        var error = await Assert.ThrowsAsync<HackerNewsUnavailableException>(
            () => client.GetItemAsync(1, CancellationToken.None));
        Assert.IsType<JsonException>(error.InnerException, exactMatch: false);
    }

    [Fact]
    public async Task Does_not_hide_a_cancel_from_the_caller()
    {
        var (client, _) = CreateClient(_ => FakeHttpMessageHandler.Json("[]"));
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => client.GetBestStoryIdsAsync(cancellation.Token));
    }
}
