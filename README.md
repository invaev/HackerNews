# Hacker News API

A RESTful API to retrieve the best `n` stories from Hacker News, implemented using ASP.NET Core.

## Features

- Retrieves the best `n` stories from Hacker News.
- Sorts stories by score in descending order.
- Efficiently fetches story details in parallel.
- Caches results to minimize load on the Hacker News API and improve performance.

## Getting Started

### Prerequisites

- .NET 8.0 SDK or later.

### Running the Application

1. Clone the repository.
2. Navigate to the project directory:
   ```bash
   cd Santander.HackerNews.Api
   ```
3. Run the application:
   ```bash
   dotnet run
   ```
4. The API will be available at `http://localhost:5296` (or the port displayed in the console).

### Running Tests

To run the unit and integration tests:

```bash
dotnet test
```

### Usage

**Endpoint:** `GET /BestStories/{n}`

**Example:** Retrieve the top 10 best stories.

```bash
curl http://localhost:5296/BestStories/10
```

**Response Format:**

```json
[
  {
    "title": "Story Title",
    "uri": "http://example.com/story",
    "postedBy": "user",
    "time": "2023-10-27T10:00:00+00:00",
    "score": 100,
    "commentCount": 50
  },
  ...
]
```

## Assumptions

- The Hacker News API `beststories.json` endpoint returns IDs in a roughly sorted order, but we explicitly sort by score to ensure correctness as per requirements.
- `n` is expected to be a reasonable number. Large values of `n` (e.g., > 100) might take longer to fetch initially but will be cached.
- If a story ID fails to fetch or is not a story (e.g., it's a job or poll, though `beststories` usually contains stories), it is skipped.

## Enhancements

Given more time, I would consider the following enhancements:

- **Resilience**: Implement retry logic (e.g., using Polly) for external API calls to handle transient failures.
- **Advanced Caching**: Use a distributed cache (e.g., Redis) for scalability across multiple instances.
- **Rate Limiting**: Implement rate limiting on this API to prevent abuse.
- **Pagination**: Support pagination for the `best-stories` endpoint if `n` is very large.
