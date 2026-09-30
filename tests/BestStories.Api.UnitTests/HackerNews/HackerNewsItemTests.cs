using System.Text.Json;
using BestStories.Api.HackerNews;

namespace BestStories.Api.UnitTests.HackerNews;

public class HackerNewsItemTests
{
    // A real answer from https://hacker-news.firebaseio.com/v0/item/21233041.json
    // (the list of comment IDs in "kids" is shorter here).
    private const string StoryJson = """
        {
          "by": "ismaildonmez",
          "descendants": 588,
          "id": 21233041,
          "kids": [21233229, 21233577],
          "score": 1757,
          "time": 1570887781,
          "title": "A uBlock Origin update was rejected from the Chrome Web Store",
          "type": "story",
          "url": "https://github.com/uBlockOrigin/uBlock-issues/issues/745"
        }
        """;

    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public void Reads_all_fields_of_a_story()
    {
        var item = JsonSerializer.Deserialize<HackerNewsItem>(StoryJson, _jsonOptions);

        Assert.NotNull(item);
        Assert.Equal(21233041, item.Id);
        Assert.Equal("story", item.Type);
        Assert.Equal("ismaildonmez", item.By);
        Assert.Equal(1570887781, item.Time);
        Assert.Equal("A uBlock Origin update was rejected from the Chrome Web Store", item.Title);
        Assert.Equal("https://github.com/uBlockOrigin/uBlock-issues/issues/745", item.Url);
        Assert.Equal(1757, item.Score);
        Assert.Equal(588, item.Descendants);
        Assert.False(item.Deleted);
        Assert.False(item.Dead);
    }

    [Fact]
    public void Reads_the_deleted_and_dead_flags()
    {
        var item = JsonSerializer.Deserialize<HackerNewsItem>(
            """{ "id": 1, "deleted": true, "dead": true }""", _jsonOptions);

        Assert.NotNull(item);
        Assert.True(item.Deleted);
        Assert.True(item.Dead);
        Assert.Null(item.Title);
        Assert.Null(item.Url);
    }

    [Fact]
    public void Null_answer_gives_no_item()
    {
        // Hacker News answers "null" when it does not know the ID.
        var item = JsonSerializer.Deserialize<HackerNewsItem>("null", _jsonOptions);

        Assert.Null(item);
    }
}
