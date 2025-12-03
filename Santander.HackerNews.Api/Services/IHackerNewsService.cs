using Santander.HackerNews.Api.Models;

namespace Santander.HackerNews.Api.Services;

public interface IHackerNewsService
{
    Task<IEnumerable<StoryDto>> GetBestStoriesAsync(int n, CancellationToken cancellationToken = default);
}
