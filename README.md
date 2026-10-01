# Hacker News Best Stories API

## Quick start

You need [Docker](https://docs.docker.com/get-docker/) and [Git](https://git-scm.com/downloads).

```bash
git clone https://github.com/Ryback2501/AscendionCodingTask.git
cd AscendionCodingTask
docker build -t beststories-api .
docker run --rm -d -p 8080:8080 --name beststories-api beststories-api
curl "http://localhost:8080/api/stories/best?n=5"
```

The first answer takes a few seconds. The next answers are almost instant. To stop the API, run `docker stop beststories-api`.

## What is this project?

This project is a small web service (an API). It gives you the best stories from [Hacker News](https://news.ycombinator.com/).

- You ask for a number of stories, for example 10.
- The API gets the best stories from the [Hacker News API](https://github.com/HackerNews/API).
- It sorts them by score. The story with the highest score comes first.
- It sends you back the stories as a list in JSON format.

The API must also answer many requests without sending too many requests to Hacker News.

The full task description is in [`docs/Backend_Developer_Coding_Test.pdf`](docs/Backend_Developer_Coding_Test.pdf).

## Technology

- .NET 10 and ASP.NET Core (a framework to build web services in C#)
- xUnit for the automatic tests
- Docker, to run the API in a container (an isolated box with everything it needs)

## What you need

You can run the API in two ways. You need only one of them:

- **With .NET:** install the [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0). Check it with `dotnet --version`. It must show `10.0.300` or newer.
- **With Docker:** install [Docker](https://docs.docker.com/get-docker/).

To run the automatic tests, you need .NET.

First, download the code with [Git](https://git-scm.com/downloads) and go into its folder. (You can also download the code as a ZIP file from the GitHub page.)

```bash
git clone https://github.com/Ryback2501/AscendionCodingTask.git
cd AscendionCodingTask
```

## How to build and test

This part needs .NET.

```bash
dotnet build
dotnet test
```

`dotnet test` runs all the automatic tests. At the end, it must say `Passed!` for each test project, and show no failed tests. The tests never call the real Hacker News. They use a fake Hacker News, so they work without internet.

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

The API starts at `http://localhost:8080`. To stop it, press `Ctrl+C`, or run `docker stop beststories-api` in another terminal.

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
  "errors": { "n": ["n is required. It must be a whole number from 1 to 200."] },
  "traceId": "00-9ae66c883902e023845a2f608d8f5a2a-728befe801a56a63-00"
}
```

The `traceId` is different for each request. It helps to find the request in the logs.

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
| `HackerNews:BestStoriesCacheSeconds` | `60` | How long the API keeps the sorted list of best stories in the cache, in seconds (1 to 3600). |
| `HackerNews:StoryCacheSeconds` | `300` | How long the API keeps each story in the cache, in seconds (1 to 86400). |

If a setting is not valid (for example, `BaseUrl` is not a web address), the API does not start. It shows an error message that names the wrong setting.

You can change a setting without editing the file. Use an environment variable (a setting of the terminal or the container). Write `__` (two underscores) in place of `:`. For example, to keep the list in the cache for 2 minutes in place of 60 seconds:

```bash
# With .NET
HackerNews__BestStoriesCacheSeconds=120 dotnet run --project src/BestStories.Api

# With Docker
docker run --rm -p 8080:8080 -e HackerNews__BestStoriesCacheSeconds=120 beststories-api
```

## How the API protects Hacker News

The task says that the API must answer many requests without sending too many requests to Hacker News.

The API reads two things from Hacker News:

- `beststories.json`: the IDs of the best stories (about 200 IDs).
- `item/<id>.json`: the details of one story (title, address, author, time, score, number of comments).

So, without protection, each request to the API would make about 201 calls to Hacker News. The API avoids this in four ways.

### 1. A cache for the sorted list

A cache is a short-term memory of answers. The API builds the full list of best stories, sorted by score, and keeps it in the cache for 60 seconds. During these 60 seconds, every request uses this list. It does not matter how many requests arrive, or which `n` they ask for. Nobody calls Hacker News.

### 2. A cache for each story

To build the list, the API needs about 200 stories. It keeps each story in the cache for 5 minutes. When the list is built again after 60 seconds, the API gets the IDs again (1 call), but it loads only the stories that are new in the list. The other stories come from the cache.

### 3. Only one build at the same time

When many requests arrive at the same time and the list is not in the cache, only one request builds the list. The other requests wait for the same result. This is called "stampede protection". The cache library (`HybridCache`, from Microsoft) does this.

### 4. Limits on the calls to Hacker News

- **Parallel limit:** the API makes at most 8 calls to Hacker News at the same time.
- **Time limits:** each call has a maximum time. A call that takes too long stops.
- **Retries:** when a call fails, the API tries again, up to 3 more times. It waits a little longer before each new try. (The `Microsoft.Extensions.Http.Resilience` package does this.)
- **Circuit breaker:** when many calls fail, the API stops calling Hacker News for a short time. This gives Hacker News time to recover, and the API answers faster instead of waiting.
- **Errors are not kept in the cache.** If a call fails, the next request tries again.

### What this means in numbers

I tested this with the real Hacker News, in Docker:

| What I did | Result |
|---|---|
| First request (empty cache) | About 3.6 seconds. The API made 201 calls to Hacker News (1 for the IDs + 200 stories). |
| Next requests (inside the 60 seconds) | About 7 milliseconds each. No calls to Hacker News. |
| 500 more requests, 50 at the same time | All answered `200 OK`. Still no new calls to Hacker News. |

So in one minute, the API makes at most about 200 calls to Hacker News (plus the retries when calls fail), and usually much fewer. The number of requests to the API does not change this.

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
| `docs/` | The task description (PDF). |

## Assumptions

- **"Best stories" means the stories in the Hacker News `beststories` list.** This list has about 200 stories. Hacker News sorts it with its own formula, not only by score. So the API loads all the stories in the list, and then sorts them by score, highest first.
- **Same score:** when two stories have the same score, they keep the Hacker News order.
- **Only stories:** the API skips items that are not stories (for example, job posts), and items that are deleted or hidden.
- **`n` is from 1 to 200**, because the Hacker News list has about 200 stories. If there are fewer stories than `n`, the API returns all of them.
- **`uri` can be `null`.** Some stories have no web address (for example, "Ask HN" questions).
- **`time` is in UTC** and uses the format `2019-10-12T13:43:01+00:00`, like in the task.
- **`commentCount`** is the Hacker News `descendants` field: all the comments, including answers to comments. It is `0` when Hacker News does not send it.
- **All or nothing:** if the API cannot load one of the stories, even after 3 more tries, it answers `503`. It does not return a list with missing stories, because the order could be wrong.
- **The data can be a little old.** The list can be up to 60 seconds old, and the score of a story can be up to 5 minutes old. I think this is fine for a list of "best" stories, because these scores change slowly. You can change both times in the settings.
- **One copy of the API.** The cache is in the memory of the app. If you run many copies of the API, each copy has its own cache.

## What I would add with more time

### Serve more requests and protect Hacker News even more

- **A shared cache for many copies of the API.** Today, each copy of the API has its own cache in its memory. With a shared cache (for example Redis, as the second level of `HybridCache`), all copies use the same cache. Then Hacker News gets one set of calls, not one set for each copy.
- **Reload only the stories that changed.** Hacker News has an `updates` address that lists the stories that changed. With it, the API could reload only these stories, not every story after 5 minutes.
- **HTTP cache headers.** Headers like `Cache-Control` and `ETag` tell browsers and other servers that they can keep the answer for a short time. Then fewer requests reach the API.
- **A limit of requests for each client (rate limiting).** ASP.NET Core has a rate limiter. With it, one client cannot send so many requests that the API becomes slow for everybody.
- **Use old data when Hacker News is down.** Today, if Hacker News is down and the list is not in the cache, the API answers `503`. It could keep the last good list for a longer time, and send it (with a note that it is old) in place of an error.
- **Load tests in the automatic checks.** A load test sends many requests at the same time (for example with the tools k6 or NBomber). It would check on every change that many requests still make only a few calls to Hacker News.

### Other improvements

- **Refresh the list in the background.** A background job could build the list when the API starts, and build it again before the cache time ends. Then no user waits about 4 seconds for the first answer. Note: this makes answers faster, but it does **not** reduce the calls to Hacker News. The API would call Hacker News even when nobody uses it.
- **Monitoring.** With OpenTelemetry (a standard way to collect measurements), I could see the number of cache hits, the number of calls to Hacker News, and the answer times.
- **A health check for Hacker News.** Today, `/health` only says that the API is running. A second check could also test if Hacker News answers.
- **An interactive API page.** Tools like Swagger UI or Scalar can show the OpenAPI description as a web page, where you can try the API in the browser.
- **API versions.** An address like `/api/v1/stories/best` would let me change the API later without breaking the programs that use the old version.
