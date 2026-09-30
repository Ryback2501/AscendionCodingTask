using BestStories.Api.HackerNews;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace BestStories.Api.IntegrationTests;

/// <summary>
/// Starts the whole API in memory, with a fake Hacker News in place of the real one.
/// </summary>
public sealed class BestStoriesApiFactory : WebApplicationFactory<Program>
{
    public FakeHackerNews HackerNews { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder) =>
        builder.ConfigureTestServices(services =>
            services.AddHttpClient<IHackerNewsClient, HackerNewsClient>()
                .ConfigurePrimaryHttpMessageHandler(HackerNews.CreateHandler));
}
