using Microsoft.AspNetCore.Mvc;
using RedditTopPostsAndUsers.Services;

namespace RedditTopPostsAndUsers.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SubredditStatisticsController : ControllerBase
    {
        // private readonly ILogger<SubredditStatisticsController> _logger;
        private readonly ISubredditStatisticsService _subredditStatisticsService;

        public SubredditStatisticsController(
            // ILogger<SubredditStatisticsController> logger,
            ISubredditStatisticsService subredditStatisticsService
        )
        {
            // _logger = logger;
            _subredditStatisticsService = subredditStatisticsService;
        }

        [HttpGet]
        [Route("{subreddit}")]
        public IActionResult GetSubredditStatistics([FromRoute] string subreddit)
        {
            var subredditStatistics = _subredditStatisticsService.GetSubredditStatistics(subreddit);

            if (subredditStatistics == null)
            {
                return NotFound($"Statistics for subreddit {subreddit} not found.  If you wish to monitor this subreddit, please ensure it is specified accordingly within the application settings.");
            }

            return Ok(subredditStatistics); // TODO: return only top N records
        }
    }
}
