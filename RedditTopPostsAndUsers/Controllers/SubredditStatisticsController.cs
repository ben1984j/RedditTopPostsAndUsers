using Microsoft.AspNetCore.Mvc;
using RedditTopPostsAndUsers.Models;
using RedditTopPostsAndUsers.Services;

namespace RedditTopPostsAndUsers.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SubredditStatisticsController : ControllerBase
    {
        private readonly ISubredditStatisticsService _subredditStatisticsService;

        public SubredditStatisticsController(
            ISubredditStatisticsService subredditStatisticsService
        )
        {
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
;
            return Ok(new SubredditStatisticsModel()
            {
                Posts = subredditStatistics?.Posts?.OrderByDescending(x => x.Upvotes).Take(10).ToList(),
                Users = subredditStatistics?.Users?.OrderByDescending(x => x.PostCount).Take(10).ToList()
            });
        }
    }
}
