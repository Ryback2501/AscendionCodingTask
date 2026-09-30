using System.Net;
using System.Text;

namespace BestStories.Api.UnitTests.HackerNews;

/// <summary>
/// A fake web server for tests. It never calls the real Hacker News.
/// For each request, it runs the given function to build the answer.
/// It also remembers every request, so tests can check them.
/// </summary>
public sealed class FakeHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> respond)
    : HttpMessageHandler
{
    private readonly List<Uri> _requestedUris = [];

    public IReadOnlyList<Uri> RequestedUris
    {
        get
        {
            lock (_requestedUris)
            {
                return [.. _requestedUris];
            }
        }
    }

    /// <summary>Builds an answer with the given status code and JSON text.</summary>
    public static HttpResponseMessage Json(string json, HttpStatusCode status = HttpStatusCode.OK) =>
        new(status) { Content = new StringContent(json, Encoding.UTF8, "application/json") };

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        lock (_requestedUris)
        {
            _requestedUris.Add(request.RequestUri!);
        }

        return Task.FromResult(respond(request));
    }
}
