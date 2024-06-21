using RedditTopPostsAndUsers.Models;

namespace RedditTopPostsAndUsers.Services
{
    public interface ISubredditStatisticsService
    {
        public SubredditStatisticsModel? GetSubredditStatistics(string subreddit);

        public Task MonitorSubreddit(string subreddit, int? maxIterations = null);
    }
}