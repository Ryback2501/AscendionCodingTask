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

// Error answers use ProblemDetails: a standard JSON format for errors.
builder.Services.AddProblemDetails();

// OpenAPI makes a description of the API that tools and people can read.
builder.Services.AddOpenApi();

var app = builder.Build();

// Turn unexpected errors and empty error answers (like 400 or 404) into ProblemDetails.
// A bad request (for example "n=abc") keeps its own status code (400), not 500.
app.UseExceptionHandler(new ExceptionHandlerOptions
{
    StatusCodeSelector = error => error is BadHttpRequestException badRequest
        ? badRequest.StatusCode
        : StatusCodes.Status500InternalServerError,
});
app.UseStatusCodePages();

// GET /health answers "Healthy" when the API is running.
app.MapHealthChecks("/health");

// GET /api/stories/best?n=10 returns the best n stories, sorted by score.
app.MapBestStoriesEndpoints();

// GET /openapi/v1.json returns the description of the API.
app.MapOpenApi();

app.Run();
