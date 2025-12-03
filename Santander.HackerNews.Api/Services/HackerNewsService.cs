using Microsoft.Extensions.Caching.Memory;
using Santander.HackerNews.Api.Models;
using System.Collections.Concurrent;
using System.Text.Json;

namespace Santander.HackerNews.Api.Services;

public class HackerNewsService : IHackerNewsService
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private const string BestStoriesCacheKey = "BestStories";
    private const string BaseUrl = "https://hacker-news.firebaseio.com/v0/";

    public HackerNewsService(HttpClient httpClient, IMemoryCache cache)
    {
        _httpClient = httpClient;
        _cache = cache;
    }

    public async Task<IEnumerable<StoryDto>> GetBestStoriesAsync(int n, CancellationToken cancellationToken = default)
    {
        if (n <= 0) return Enumerable.Empty<StoryDto>();

        // 1. Get Best Story IDs (Cached)
        if (!_cache.TryGetValue(BestStoriesCacheKey, out List<int>? bestStoryIds))
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}beststories.json", cancellationToken);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            bestStoryIds = JsonSerializer.Deserialize<List<int>>(content) ?? new List<int>();

            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(5)); // Cache IDs for 5 mins

            _cache.Set(BestStoriesCacheKey, bestStoryIds, cacheEntryOptions);
        }

        if (bestStoryIds == null || !bestStoryIds.Any()) return Enumerable.Empty<StoryDto>();

        // 2. Fetch details for the top n stories
        // We might need to fetch more than n if some are not stories or fail, but usually beststories are stories.
        // For simplicity and efficiency, we'll take the top n IDs.
        var targetIds = bestStoryIds.Take(n).ToList();
        var stories = new ConcurrentBag<StoryDto>();

        await Parallel.ForEachAsync(targetIds, new ParallelOptions { MaxDegreeOfParallelism = 20, CancellationToken = cancellationToken }, async (id, ct) =>
        {
            var story = await GetStoryDetailsAsync(id, ct);
            if (story != null)
            {
                stories.Add(story);
            }
        });

        return stories.OrderByDescending(s => s.Score).ToList();
    }

    private async Task<StoryDto?> GetStoryDetailsAsync(int id, CancellationToken cancellationToken)
    {
        string cacheKey = $"Story_{id}";
        if (!_cache.TryGetValue(cacheKey, out StoryDto? storyDto))
        {
            try
            {
                var response = await _httpClient.GetAsync($"{BaseUrl}item/{id}.json", cancellationToken);
                if (!response.IsSuccessStatusCode) return null;

                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                var item = JsonSerializer.Deserialize<HackerNewsItem>(content);

                if (item == null || item.Type != "story") return null;

                storyDto = MapToDto(item);

                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(10)); // Cache details for 10 mins

                _cache.Set(cacheKey, storyDto, cacheEntryOptions);
            }
            catch
            {
                // Log error or ignore
                return null;
            }
        }

        return storyDto;
    }

    private static StoryDto MapToDto(HackerNewsItem item)
    {
        return new StoryDto
        {
            Title = item.Title,
            Uri = item.Url,
            PostedBy = item.By,
            Time = DateTimeOffset.FromUnixTimeSeconds(item.Time).ToString("yyyy-MM-ddTHH:mm:ss+00:00"),
            Score = item.Score,
            CommentCount = item.Descendants
        };
    }
}
