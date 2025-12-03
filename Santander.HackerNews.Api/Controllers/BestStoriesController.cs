using Microsoft.AspNetCore.Mvc;
using Santander.HackerNews.Api.Models;
using Santander.HackerNews.Api.Services;

namespace Santander.HackerNews.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class BestStoriesController : ControllerBase
{
    private readonly IHackerNewsService _hackerNewsService;

    public BestStoriesController(IHackerNewsService hackerNewsService)
    {
        _hackerNewsService = hackerNewsService;
    }

    [HttpGet("{n}")]
    public async Task<ActionResult<IEnumerable<StoryDto>>> Get(int n, CancellationToken cancellationToken)
    {
        if (n <= 0)
        {
            return BadRequest("n must be greater than 0");
        }

        var stories = await _hackerNewsService.GetBestStoriesAsync(n, cancellationToken);
        return Ok(stories);
    }
}
