using BestStories.Api.HackerNews;
using BestStories.Api.Stories;
using Microsoft.Extensions.Options;

namespace BestStories.Api.UnitTests.Stories;

public class BestStoriesServiceTests
{
    private readonly FakeHackerNewsClient _hackerNews = new();

    private BestStoriesService CreateService(int maxParallelRequests = 8) =>
        new(_hackerNews, Options.Create(new HackerNewsOptions { MaxParallelRequests = maxParallelRequests }));

    [Fact]
    public async Task Sorts_by_score_and_takes_n_stories()
    {
        _hackerNews.AddStory(id: 1, score: 10);
        _hackerNews.AddStory(id: 2, score: 30);
        _hackerNews.AddStory(id: 3, score: 20);
        _hackerNews.AddStory(id: 4, score: 5);

        var stories = await CreateService().GetBestStoriesAsync(3, CancellationToken.None);

        Assert.Equal([30, 20, 10], stories.Select(story => story.Score));
    }

    [Fact]
    public async Task Keeps_the_hacker_news_order_when_scores_are_the_same()
    {
        _hackerNews.AddStory(id: 7, score: 50);
        _hackerNews.AddStory(id: 3, score: 50);
        _hackerNews.AddStory(id: 5, score: 50);

        var stories = await CreateService().GetBestStoriesAsync(3, CancellationToken.None);

        Assert.Equal(["Story 7", "Story 3", "Story 5"], stories.Select(story => story.Title));
    }

    [Fact]
    public async Task Skips_items_that_are_not_real_stories()
    {
        _hackerNews.AddStory(id: 1, score: 10);
        _hackerNews.AddStory(id: 2, score: 90, type: "job");
        _hackerNews.BestStoryIds.Add(3);
        _hackerNews.Items[3] = null; // Hacker News does not know this ID.
        _hackerNews.BestStoryIds.Add(4);
        _hackerNews.Items[4] = new HackerNewsItem { Id = 4, Type = "story", Score = 80, Deleted = true };
        _hackerNews.BestStoryIds.Add(5);
        _hackerNews.Items[5] = new HackerNewsItem { Id = 5, Type = "story", Score = 70, Dead = true };

        var stories = await CreateService().GetBestStoriesAsync(10, CancellationToken.None);

        Assert.Equal("Story 1", Assert.Single(stories).Title);
    }

    [Fact]
    public async Task Maps_every_field()
    {
        _hackerNews.BestStoryIds.Add(21233041);
        _hackerNews.Items[21233041] = new HackerNewsItem
        {
            Id = 21233041,
            Type = "story",
            By = "ismaildonmez",
            Time = 1570887781,
            Title = "A uBlock Origin update was rejected from the Chrome Web Store",
            Url = "https://github.com/uBlockOrigin/uBlock-issues/issues/745",
            Score = 1716,
            Descendants = 572,
        };

        var story = Assert.Single(await CreateService().GetBestStoriesAsync(1, CancellationToken.None));

        Assert.Equal("A uBlock Origin update was rejected from the Chrome Web Store", story.Title);
        Assert.Equal("https://github.com/uBlockOrigin/uBlock-issues/issues/745", story.Uri);
        Assert.Equal("ismaildonmez", story.PostedBy);
        Assert.Equal(new DateTimeOffset(2019, 10, 12, 13, 43, 1, TimeSpan.Zero), story.Time);
        Assert.Equal(TimeSpan.Zero, story.Time.Offset);
        Assert.Equal(1716, story.Score);
        Assert.Equal(572, story.CommentCount);
    }

    [Fact]
    public async Task Uses_empty_values_when_hacker_news_leaves_out_fields()
    {
        // For example, an "Ask HN" story has no web address.
        _hackerNews.BestStoryIds.Add(1);
        _hackerNews.Items[1] = new HackerNewsItem { Id = 1, Type = "story", Score = 5 };

        var story = Assert.Single(await CreateService().GetBestStoriesAsync(1, CancellationToken.None));

        Assert.Equal("", story.Title);
        Assert.Null(story.Uri);
        Assert.Equal("", story.PostedBy);
        Assert.Equal(0, story.CommentCount);
    }

    [Fact]
    public async Task Returns_all_stories_when_n_is_bigger_than_the_list()
    {
        _hackerNews.AddStory(id: 1, score: 10);
        _hackerNews.AddStory(id: 2, score: 20);

        var stories = await CreateService().GetBestStoriesAsync(200, CancellationToken.None);

        Assert.Equal(2, stories.Count);
    }

    [Fact]
    public async Task Passes_on_the_hacker_news_error()
    {
        _hackerNews.AddStory(id: 1, score: 10);
        _hackerNews.ItemError = new HackerNewsUnavailableException("Test error", new HttpRequestException());

        await Assert.ThrowsAsync<HackerNewsUnavailableException>(
            () => CreateService().GetBestStoriesAsync(1, CancellationToken.None));
    }

    [Fact]
    public async Task Never_makes_more_calls_at_the_same_time_than_the_limit()
    {
        for (var id = 1; id <= 30; id++)
        {
            _hackerNews.AddStory(id, score: id);
        }

        _hackerNews.ItemDelay = TimeSpan.FromMilliseconds(20);

        var stories = await CreateService(maxParallelRequests: 4).GetBestStoriesAsync(30, CancellationToken.None);

        Assert.Equal(30, stories.Count);
        Assert.InRange(_hackerNews.MostCallsAtTheSameTime, 2, 4);
    }
}
