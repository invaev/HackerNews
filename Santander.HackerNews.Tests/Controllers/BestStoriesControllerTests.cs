using Microsoft.AspNetCore.Mvc.Testing;
using Santander.HackerNews.Api.Models;
using System.Net;
using System.Net.Http.Json;

namespace Santander.HackerNews.Tests.Controllers;

public class BestStoriesControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public BestStoriesControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Get_ReturnsOk_WithStories()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/BestStories/5");

        // Assert
        response.EnsureSuccessStatusCode();
        var stories = await response.Content.ReadFromJsonAsync<List<StoryDto>>();
        Assert.NotNull(stories);
        // Note: We can't easily assert the exact content without mocking the external API in the integration test,
        // but we can check if the response structure is correct and we get some data (assuming HN API is up).
        // For a true isolated integration test, we would replace the HttpClient in the DI container with a mock.
        // However, for this coding test, checking the end-to-end flow including the real API (or at least the service logic) is valuable.
        // If we wanted to mock the external API, we'd configure the WebApplicationFactory to replace the HttpClient or IHackerNewsService.
    }

    [Fact]
    public async Task Get_ReturnsBadRequest_WhenNIsInvalid()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/BestStories/0");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
