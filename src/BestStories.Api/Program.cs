// This file starts the web API.
using BestStories.Api.HackerNews;
using BestStories.Api.Stories;

var builder = WebApplication.CreateBuilder(args);

// Health checks tell us if the API is running.
builder.Services.AddHealthChecks();

// The client that reads stories from Hacker News.
builder.Services.AddHackerNewsClient(builder.Configuration);

// The service that finds the best stories by score.
builder.Services.AddScoped<IBestStoriesService, BestStoriesService>();

// OpenAPI makes a description of the API that tools and people can read.
builder.Services.AddOpenApi();

var app = builder.Build();

// GET /health answers "Healthy" when the API is running.
app.MapHealthChecks("/health");

// GET /openapi/v1.json returns the description of the API.
app.MapOpenApi();

app.Run();
