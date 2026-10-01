using BestStories.Api.HackerNews;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace BestStories.Api.UnitTests.HackerNews;

public class HackerNewsRegistrationTests
{
    private static ServiceProvider BuildServices(string? baseUrl)
    {
        var settings = new Dictionary<string, string?>();
        if (baseUrl is not null)
        {
            settings["HackerNews:BaseUrl"] = baseUrl;
        }

        var configuration = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
        var services = new ServiceCollection();
        services.AddHackerNewsClient(configuration);
        return services.BuildServiceProvider();
    }

    [Fact]
    public void Uses_the_real_hacker_news_address_when_nothing_is_set()
    {
        using var services = BuildServices(baseUrl: null);

        var options = services.GetRequiredService<IOptions<HackerNewsOptions>>().Value;

        Assert.Equal("https://hacker-news.firebaseio.com/v0/", options.BaseUrl);
    }

    [Fact]
    public void Uses_the_address_from_the_settings()
    {
        using var services = BuildServices("https://hn.test/v0/");

        var options = services.GetRequiredService<IOptions<HackerNewsOptions>>().Value;

        Assert.Equal("https://hn.test/v0/", options.BaseUrl);
    }

    [Fact]
    public void Rejects_an_address_that_is_not_a_url()
    {
        using var services = BuildServices("not-a-url");

        Assert.Throws<OptionsValidationException>(
            () => services.GetRequiredService<IOptions<HackerNewsOptions>>().Value);
    }

    [Theory]
    [InlineData("MaxParallelRequests", "0")]
    [InlineData("MaxParallelRequests", "51")]
    [InlineData("StoryCacheSeconds", "0")]
    [InlineData("StoryCacheSeconds", "86401")]
    [InlineData("BestStoriesCacheSeconds", "0")]
    [InlineData("BestStoriesCacheSeconds", "3601")]
    public void Rejects_a_number_outside_its_allowed_range(string setting, string value)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { [$"HackerNews:{setting}"] = value })
            .Build();
        var services = new ServiceCollection();
        services.AddHackerNewsClient(configuration);
        using var provider = services.BuildServiceProvider();

        Assert.Throws<OptionsValidationException>(
            () => provider.GetRequiredService<IOptions<HackerNewsOptions>>().Value);
    }

    [Fact]
    public async Task Adds_the_missing_slash_at_the_end_of_the_address()
    {
        var handler = new FakeHttpMessageHandler(_ => FakeHttpMessageHandler.Json("[]"));
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["HackerNews:BaseUrl"] = "https://hn.test/v0" })
            .Build();
        var services = new ServiceCollection();
        services.AddHackerNewsClient(configuration)
            .ConfigurePrimaryHttpMessageHandler(() => handler);
        using var provider = services.BuildServiceProvider();

        await provider.GetRequiredService<IHackerNewsClient>().GetBestStoryIdsAsync(CancellationToken.None);

        Assert.Equal(new Uri("https://hn.test/v0/beststories.json"), Assert.Single(handler.RequestedUris));
    }

    [Fact]
    public void Registers_the_client()
    {
        using var services = BuildServices("https://hn.test/v0/");

        var client = services.GetRequiredService<IHackerNewsClient>();

        Assert.IsType<HackerNewsClient>(client);
    }
}
