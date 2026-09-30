# Hacker News Best Stories API

> **Status: work in progress.** The endpoint that returns the best stories works. The next step
> is a cache (a short-term memory of answers). Without the cache, each request makes about 200
> calls to Hacker News, and it takes a few seconds.

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
| `GET /api/stories/best?n=10` | Gives the best `n` stories from Hacker News, sorted by score (highest first). |
| `GET /health` | Tells you if the API is running. It answers `Healthy`. |
| `GET /openapi/v1.json` | Gives a description of the API in the OpenAPI format (a standard format that tools can read). |

### Get the best stories

`n` is the number of stories you want. It must be a whole number from 1 to 200.

```bash
curl "http://localhost:5000/api/stories/best?n=2"
```

You get a list like this one (example data):

```json
[
  {
    "title": "A uBlock Origin update was rejected from the Chrome Web Store",
    "uri": "https://github.com/uBlockOrigin/uBlock-issues/issues/745",
    "postedBy": "ismaildonmez",
    "time": "2019-10-12T13:43:01+00:00",
    "score": 1716,
    "commentCount": 572
  },
  {
    "title": "Ask HN: What are you working on?",
    "uri": null,
    "postedBy": "someone",
    "time": "2019-10-12T09:15:00+00:00",
    "score": 1500,
    "commentCount": 820
  }
]
```

| Field | What it means |
|---|---|
| `title` | The title of the story. |
| `uri` | The web address of the story. It is `null` when the story has no address (for example, "Ask HN" questions). |
| `postedBy` | The user name of the author. |
| `time` | When the story was posted, in UTC time. |
| `score` | The score of the story on Hacker News. |
| `commentCount` | The total number of comments. |

### Error answers

Errors use ProblemDetails, a standard JSON format for errors (RFC 9457).

| Status | When |
|---|---|
| `400 Bad Request` | `n` is missing, is not a whole number, or is not between 1 and 200. |
| `503 Service Unavailable` | The API cannot get the stories from Hacker News, even after 3 more tries. |

Example:

```bash
curl "http://localhost:5000/api/stories/best?n=0"
```

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "The value of n is not valid.",
  "status": 400,
  "errors": { "n": ["n is required. It must be a whole number from 1 to 200."] }
}
```

### Check that the API is running

```bash
curl http://localhost:5000/health
```

You should see: `Healthy`

## Configuration

The settings are in `src/BestStories.Api/appsettings.json`.

| Setting | Default value | What it does |
|---|---|---|
| `HackerNews:BaseUrl` | `https://hacker-news.firebaseio.com/v0/` | The address of the Hacker News API. |
| `HackerNews:MaxParallelRequests` | `8` | The highest number of calls to Hacker News at the same time (1 to 50). |

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
| `src/BestStories.Api/Stories/` | The best stories endpoint, and the code that sorts the stories by score. |
| `tests/BestStories.Api.UnitTests/` | Tests for small parts of the code, one part at a time. |
| `tests/BestStories.Api.IntegrationTests/` | Tests that start the whole API and call it over HTTP. |
| `Directory.Build.props` | Settings for all projects, and the version number of the app. |
| `Dockerfile` | The instructions to build the Docker image. |
| `.github/workflows/` | The automatic checks that run on GitHub for every change. |

## Assumptions

- **"Best stories" means the stories in the Hacker News `beststories` list.** This list has about
  200 stories. Hacker News sorts it with its own formula, not only by score. So the API loads all
  the stories in the list, and then sorts them by score, highest first.
- **Same score:** when two stories have the same score, they keep the Hacker News order.
- **Only stories:** the API skips items that are not stories (for example, job posts), and items
  that are deleted or hidden.
- **`n` is from 1 to 200**, because the Hacker News list has about 200 stories. If there are fewer
  stories than `n`, the API returns all of them.
- **`uri` can be `null`.** Some stories have no web address (for example, "Ask HN" questions).
- **`time` is in UTC** and uses the format `2019-10-12T13:43:01+00:00`, like in the task.
- **`commentCount`** is the Hacker News `descendants` field: all the comments, including answers
  to comments. It is `0` when Hacker News does not send it.
- **All or nothing:** if the API cannot load one of the stories, even after 3 more tries, it
  answers `503`. It does not return a list with missing stories, because the order could be wrong.

## What we would add with more time

Coming soon.
