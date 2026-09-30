# Hacker News Best Stories API

> **Status: work in progress.** The code is not ready yet. This file will grow with each step.

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

## How to run it

Coming soon. This section will explain how to build, test and run the service.

## Assumptions

Coming soon.

## What we would add with more time

Coming soon.
