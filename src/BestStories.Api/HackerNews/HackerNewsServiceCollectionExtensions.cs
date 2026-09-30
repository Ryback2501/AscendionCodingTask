using Microsoft.Extensions.Options;

namespace BestStories.Api.HackerNews;

public static class HackerNewsServiceCollectionExtensions
{
    /// <summary>
    /// Adds the Hacker News settings and the Hacker News client to the app.
    /// The app stops at start if the settings are not valid.
    /// It returns the HTTP client builder, so tests can replace the real network with a fake one.
    /// </summary>
    public static IHttpClientBuilder AddHackerNewsClient(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<HackerNewsOptions>()
            .Bind(configuration.GetSection(HackerNewsOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return services.AddHttpClient<IHackerNewsClient, HackerNewsClient>((serviceProvider, httpClient) =>
        {
            var baseUrl = serviceProvider.GetRequiredService<IOptions<HackerNewsOptions>>().Value.BaseUrl;

            // The address must end with "/". Without it, "item/1.json" would replace the last part of the address.
            httpClient.BaseAddress = new Uri(baseUrl.EndsWith('/') ? baseUrl : baseUrl + "/");
        });
    }
}
