using System.Net;
using BestStories.Api.HackerNews;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;

namespace BestStories.Api.UnitTests.HackerNews;

public class HackerNewsResilienceTests
{
    // Builds the real client setup from the app, with a fake Hacker News behind it.
    private static ServiceProvider BuildServices(FakeHttpMessageHandler handler)
    {
        var configuration = new ConfigurationBuilder().Build();
        var services = new ServiceCollection();
        var clientBuilder = services.AddHackerNewsClient(configuration)
            .ConfigurePrimaryHttpMessageHandler(() => handler);

        // Wait almost no time between tries, so the test stays fast.
        services.Configure<HttpStandardResilienceOptions>(
            $"{clientBuilder.Name}-standard",
            options => options.Retry.Delay = TimeSpan.FromMilliseconds(1));

        return services.BuildServiceProvider();
    }

    [Fact]
    public async Task Tries_again_when_hacker_news_fails_once()
    {
        var calls = 0;
        var handler = new FakeHttpMessageHandler(_ => Interlocked.Increment(ref calls) == 1
            ? FakeHttpMessageHandler.Json("{}", HttpStatusCode.ServiceUnavailable)
            : FakeHttpMessageHandler.Json("[1, 2, 3]"));
        using var services = BuildServices(handler);

        var ids = await services.GetRequiredService<IHackerNewsClient>()
            .GetBestStoryIdsAsync(CancellationToken.None);

        Assert.Equal([1, 2, 3], ids);
        Assert.Equal(2, handler.RequestedUris.Count);
    }

    [Fact]
    public async Task Stops_trying_after_the_last_retry()
    {
        var handler = new FakeHttpMessageHandler(_ =>
            FakeHttpMessageHandler.Json("{}", HttpStatusCode.ServiceUnavailable));
        using var services = BuildServices(handler);

        await Assert.ThrowsAsync<HttpRequestException>(() => services
            .GetRequiredService<IHackerNewsClient>()
            .GetBestStoryIdsAsync(CancellationToken.None));

        // 1 first try + 3 retries.
        Assert.Equal(4, handler.RequestedUris.Count);
    }
}
