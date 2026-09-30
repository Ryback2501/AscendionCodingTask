using System.Net;
using System.Text.Json;

namespace BestStories.Api.IntegrationTests;

public class HackerNewsDownTests
{
    [Fact]
    public async Task Answers_503_with_problem_details_when_hacker_news_is_down()
    {
        using var api = new BestStoriesApiFactory();
        api.HackerNews.IsDown = true;

        var response = await api.CreateClient().GetAsync("/api/stories/best?n=5");

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(503, json.RootElement.GetProperty("status").GetInt32());
        Assert.Equal(
            "Hacker News is not available. Please try again later.",
            json.RootElement.GetProperty("title").GetString());
    }
}
