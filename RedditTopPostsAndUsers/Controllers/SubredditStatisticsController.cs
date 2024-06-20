using Microsoft.AspNetCore.Mvc;

namespace RedditTopPostsAndUsers.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SubredditStatisticsController : ControllerBase
    {
        private readonly ILogger<SubredditStatisticsController> _logger;
        private readonly ISubredditStatisticsRepository _subredditStatisticsRepository;

        public SubredditStatisticsController(
            ILogger<SubredditStatisticsController> logger,
            ISubredditStatisticsRepository subredditStatisticsRepository
        )
        {
            _logger = logger;
            _subredditStatisticsRepository = subredditStatisticsRepository;
        }

        [HttpGet]
        [Route("{subreddit}")]
        public IActionResult Get([FromRoute] string subreddit)
        {
            var subredditStatistics = _subredditStatisticsRepository.GetStatistics(subreddit);

            if (subredditStatistics == null)
            {
                return NotFound($"Statistics for subreddit {subreddit} not found.  If you wish to monitor this subreddit, please ensure it is specified accordingly within the application settings.");
            }

            return Ok(subredditStatistics); // TODO: return only top N records
        }
    }
}
