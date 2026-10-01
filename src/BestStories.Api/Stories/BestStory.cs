using System.ComponentModel;

namespace BestStories.Api.Stories;

/// <summary>One story in the answer of GET /api/stories/best. It never changes after it is made.</summary>
/// <param name="Title">The title of the story.</param>
/// <param name="Uri">The web address of the story. It is null when the story has no address (like "Ask HN").</param>
/// <param name="PostedBy">The user name of the author.</param>
/// <param name="Time">The time when the story was posted, in UTC.</param>
/// <param name="Score">The score of the story on Hacker News.</param>
/// <param name="CommentCount">The total number of comments.</param>
[ImmutableObject(true)]
public sealed record BestStory(
    string Title,
    string? Uri,
    string PostedBy,
    DateTimeOffset Time,
    int Score,
    int CommentCount);
