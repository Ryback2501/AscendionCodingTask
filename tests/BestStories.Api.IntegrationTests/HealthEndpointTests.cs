using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BestStories.Api.IntegrationTests;

// These tests start the whole API in memory and call it over HTTP.
public class HealthEndpointTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task Health_endpoint_returns_200_ok()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
