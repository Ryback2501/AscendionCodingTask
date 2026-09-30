namespace BestStories.Api.HackerNews;

/// <summary>
/// The error when we cannot get good data from Hacker News.
/// For example: Hacker News sends an error, sends broken data, is too slow,
/// or the circuit breaker is open after many errors.
/// The first error is in InnerException.
/// </summary>
public sealed class HackerNewsUnavailableException(string message, Exception innerException)
    : Exception(message, innerException);
