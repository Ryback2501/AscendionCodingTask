# Hacker News Best Stories API

> **Status: work in progress.** The API starts and has a health check. It also has the code that
> reads stories from Hacker News. The endpoint that returns the best stories is not ready yet.

## What is this project?

This project is a small web service (an API). It gives you the best stories from
[Hacker News](https://news.ycombinator.com/).

- You ask for a number of stories, for example 10.
- The service gets the best stories from the
  [Hacker News API](https://github.com/HackerNews/API).
- It sorts them by score. The story with the highest score comes first.
- It sends you back the stories as a list in JSON format.

The service must also answer many requests without sending too many requests to
Hacker News.

The full task description is in
[`docs/Backend_Developer_Coding_Test.pdf`](docs/Backend_Developer_Coding_Test.pdf).

## Technology

- .NET 10 and ASP.NET Core (a framework to build web services in C#)
- xUnit for the automatic tests
- Docker, to run the service in a container (an isolated box with everything it needs)

## What you need

You can run the API in two ways. You need only one of them:

- **With .NET:** install the [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0).
  Check it with `dotnet --version`. It must show `10.0.300` or newer.
- **With Docker:** install [Docker](https://docs.docker.com/get-docker/).

First, download the code and go into its folder:

```bash
git clone https://github.com/Ryback2501/AscendionCodingTask.git
cd AscendionCodingTask
```

## How to build and test

```bash
dotnet build
dotnet test
```

`dotnet test` runs all the automatic tests. At the end, it must say `Passed!` for each test
project, and show no failed tests. The tests never call the real Hacker News. They use a fake
Hacker News, so they work without internet.

## How to run the API

### Option 1: with .NET

```bash
dotnet run --project src/BestStories.Api
```

The API starts at `http://localhost:5000`. To stop it, press `Ctrl+C`.

### Option 2: with Docker

```bash
docker build -t beststories-api .
docker run --rm -p 8080:8080 --name beststories-api beststories-api
```

The API starts at `http://localhost:8080`. To stop it, press `Ctrl+C`, or run
`docker stop beststories-api` in another terminal.

## How to try the API

In the commands below, use port `5000` with .NET, or port `8080` with Docker.

| Address | What it does |
|---|---|
| `GET /health` | Tells you if the API is running. It answers `Healthy`. |
| `GET /openapi/v1.json` | Gives a description of the API in the OpenAPI format (a standard format that tools can read). |

Example:

```bash
curl http://localhost:5000/health
```

You should see: `Healthy`

## Configuration

The settings are in `src/BestStories.Api/appsettings.json`.

| Setting | Default value | What it does |
|---|---|---|
| `HackerNews:BaseUrl` | `https://hacker-news.firebaseio.com/v0/` | The address of the Hacker News API. |

If a setting is not valid (for example, `BaseUrl` is not a web address), the API does not start.
It shows an error message that names the wrong setting.

You can change a setting without editing the file. Use an environment variable (a setting of the
terminal or the container). Write `__` (two underscores) in place of `:`. Examples:

```bash
# With .NET
HackerNews__BaseUrl=https://hacker-news.firebaseio.com/v0/ dotnet run --project src/BestStories.Api

# With Docker
docker run --rm -p 8080:8080 -e HackerNews__BaseUrl=https://hacker-news.firebaseio.com/v0/ beststories-api
```

## How the API talks to Hacker News

The API reads two things from Hacker News:

- `beststories.json`: the IDs of the best stories (about 200 IDs).
- `item/<id>.json`: the details of one story (title, address, author, time, score, number of comments).

Sometimes a network call fails, or Hacker News is slow. The API uses the standard .NET protection
for this (the `Microsoft.Extensions.Http.Resilience` package):

- **Time limits:** each call has a maximum time. A call that takes too long stops.
- **Retries:** when a call fails, the API tries again, up to 3 more times. It waits a little
  longer before each new try.
- **Circuit breaker:** when many calls fail, the API stops calling Hacker News for a short time.
  This gives Hacker News time to recover, and our API answers faster instead of waiting.

## Project layout

| Folder or file | What it has |
|---|---|
| `src/BestStories.Api/` | The code of the API. |
| `src/BestStories.Api/HackerNews/` | The code that reads data from the Hacker News API. |
| `tests/BestStories.Api.UnitTests/` | Tests for small parts of the code, one part at a time. |
| `tests/BestStories.Api.IntegrationTests/` | Tests that start the whole API and call it over HTTP. |
| `Directory.Build.props` | Settings for all projects, and the version number of the app. |
| `Dockerfile` | The instructions to build the Docker image. |
| `.github/workflows/` | The automatic checks that run on GitHub for every change. |

## Assumptions

Coming soon.

## What we would add with more time

Coming soon.
