using System.Net;

namespace BestStories.Api.IntegrationTests;

public class ManyRequestsTests
{
    [Fact]
    public async Task Many_requests_at_the_same_time_make_only_one_set_of_calls_to_hacker_news()
    {
        using var api = new BestStoriesApiFactory();
        const int StoryCount = 20;
        for (var id = 1; id <= StoryCount; id++)
        {
            api.HackerNews.AddStory(id, score: id * 10, $"Story {id}", $"https://story{id}.test", "author", 1570887781, id);
        }

        var client = api.CreateClient();

        // 100 requests at the same time, with different values of n.
        var requests = Enumerable.Range(1, 100)
            .Select(i => client.GetAsync($"/api/stories/best?n={i % StoryCount + 1}"));
        var responses = await Task.WhenAll(requests);

        Assert.All(responses, response => Assert.Equal(HttpStatusCode.OK, response.StatusCode));
        // 1 call for the list of IDs + 1 call for each story.
        Assert.Equal(1 + StoryCount, api.HackerNews.RequestCount);

        // One more request later: the answer comes from the cache, with no new call.
        var lastResponse = await client.GetAsync("/api/stories/best?n=5");
        Assert.Equal(HttpStatusCode.OK, lastResponse.StatusCode);
        Assert.Equal(1 + StoryCount, api.HackerNews.RequestCount);
    }
}
