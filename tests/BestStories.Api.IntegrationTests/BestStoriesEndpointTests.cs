using System.Net;
using System.Text.Json;

namespace BestStories.Api.IntegrationTests;

// Each test starts its own API with its own fake Hacker News, so the tests do not share data.
public class BestStoriesEndpointTests
{
    [Fact]
    public async Task Returns_the_stories_in_the_shape_of_the_task()
    {
        using var api = new BestStoriesApiFactory();
        api.HackerNews.AddStory(
            id: 21233041,
            score: 1716,
            title: "A uBlock Origin update was rejected from the Chrome Web Store",
            url: "https://github.com/uBlockOrigin/uBlock-issues/issues/745",
            by: "ismaildonmez",
            time: 1570887781,
            descendants: 572);

        var response = await api.CreateClient().GetAsync("/api/stories/best?n=1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var story = Assert.Single(json.RootElement.EnumerateArray());
        Assert.Equal(
            ["title", "uri", "postedBy", "time", "score", "commentCount"],
            story.EnumerateObject().Select(property => property.Name));
        Assert.Equal("A uBlock Origin update was rejected from the Chrome Web Store", story.GetProperty("title").GetString());
        Assert.Equal("https://github.com/uBlockOrigin/uBlock-issues/issues/745", story.GetProperty("uri").GetString());
        Assert.Equal("ismaildonmez", story.GetProperty("postedBy").GetString());
        Assert.Equal("2019-10-12T13:43:01+00:00", story.GetProperty("time").GetString());
        Assert.Equal(1716, story.GetProperty("score").GetInt32());
        Assert.Equal(572, story.GetProperty("commentCount").GetInt32());
    }

    [Fact]
    public async Task Returns_the_best_n_stories_with_the_highest_score_first()
    {
        using var api = new BestStoriesApiFactory();
        api.HackerNews.AddStory(1, score: 100, "One", "https://one.test", "a", 1570887781, 1);
        api.HackerNews.AddStory(2, score: 300, "Two", "https://two.test", "b", 1570887781, 2);
        api.HackerNews.AddStory(3, score: 200, "Three", null, "c", 1570887781, null);
        api.HackerNews.AddStory(4, score: 50, "Four", "https://four.test", "d", 1570887781, 4);

        var stories = await GetStoriesAsync(api, "/api/stories/best?n=3");

        Assert.Equal(["Two", "Three", "One"], stories.Select(story => story.GetProperty("title").GetString()));
        Assert.Equal(JsonValueKind.Null, stories[1].GetProperty("uri").ValueKind);
        Assert.Equal(0, stories[1].GetProperty("commentCount").GetInt32());
    }

    [Fact]
    public async Task Returns_all_stories_when_n_is_bigger_than_the_list()
    {
        using var api = new BestStoriesApiFactory();
        api.HackerNews.AddStory(1, score: 100, "One", "https://one.test", "a", 1570887781, 1);
        api.HackerNews.AddStory(2, score: 300, "Two", "https://two.test", "b", 1570887781, 2);

        var stories = await GetStoriesAsync(api, "/api/stories/best?n=200");

        Assert.Equal(2, stories.Count);
    }

    [Theory]
    [InlineData("/api/stories/best")]
    [InlineData("/api/stories/best?n=0")]
    [InlineData("/api/stories/best?n=-5")]
    [InlineData("/api/stories/best?n=201")]
    [InlineData("/api/stories/best?n=abc")]
    public async Task Answers_400_with_problem_details_when_n_is_not_valid(string address)
    {
        using var api = new BestStoriesApiFactory();

        var response = await api.CreateClient().GetAsync(address);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(400, json.RootElement.GetProperty("status").GetInt32());
        Assert.Equal(0, api.HackerNews.RequestCount);
    }

    private static async Task<List<JsonElement>> GetStoriesAsync(BestStoriesApiFactory api, string address)
    {
        var response = await api.CreateClient().GetAsync(address);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return [.. json.RootElement.EnumerateArray()];
    }
}
