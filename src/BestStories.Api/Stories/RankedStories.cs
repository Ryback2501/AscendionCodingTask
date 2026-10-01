using System.ComponentModel;

namespace BestStories.Api.Stories;

/// <summary>
/// All the best stories, sorted by score (highest first). We keep this list in the cache.
/// ImmutableObject(true) tells the cache that the list never changes, so the cache can
/// give the same object again, without making a copy.
/// </summary>
[ImmutableObject(true)]
public sealed record RankedStories(IReadOnlyList<BestStory> Stories);
