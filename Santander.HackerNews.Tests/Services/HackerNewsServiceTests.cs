using Microsoft.Extensions.Caching.Memory;
using Moq;
using Moq.Protected;
using Santander.HackerNews.Api.Services;
using System.Net;
using System.Text.Json;

namespace Santander.HackerNews.Tests.Services;

public class HackerNewsServiceTests
{
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _memoryCache;
    private readonly HackerNewsService _service;

    public HackerNewsServiceTests()
    {
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("https://hacker-news.firebaseio.com/v0/")
        };
        _memoryCache = new MemoryCache(new MemoryCacheOptions());
        _service = new HackerNewsService(_httpClient, _memoryCache);
    }

    [Fact]
    public async Task GetBestStoriesAsync_ReturnsEmpty_WhenNIsZeroOrNegative()
    {
        var result = await _service.GetBestStoriesAsync(0);
        Assert.Empty(result);

        result = await _service.GetBestStoriesAsync(-1);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetBestStoriesAsync_ReturnsStories_WhenApiReturnsData()
    {
        // Arrange
        var bestStoryIds = new List<int> { 1, 2 };
        SetupHttpResponse("beststories.json", JsonSerializer.Serialize(bestStoryIds));

        var story1 = new { title = "Story 1", url = "http://url1.com", by = "user1", time = 1234567890, score = 100, descendants = 10, type = "story", id = 1 };
        SetupHttpResponse("item/1.json", JsonSerializer.Serialize(story1));

        var story2 = new { title = "Story 2", url = "http://url2.com", by = "user2", time = 1234567890, score = 200, descendants = 20, type = "story", id = 2 };
        SetupHttpResponse("item/2.json", JsonSerializer.Serialize(story2));

        // Act
        var result = await _service.GetBestStoriesAsync(2);

        // Assert
        Assert.Equal(2, result.Count());
        var first = result.First();
        Assert.Equal("Story 2", first.Title); // Ordered by score descending
        Assert.Equal(200, first.Score);
    }

    private void SetupHttpResponse(string requestUri, string content)
    {
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri!.ToString().EndsWith(requestUri)),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(content)
            });
    }
}
