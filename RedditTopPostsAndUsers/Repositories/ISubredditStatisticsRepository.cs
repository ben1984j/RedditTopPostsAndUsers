using RedditTopPostsAndUsers.Models;

namespace RedditTopPostsAndUsers.Repositories
{
    public interface ISubredditStatisticsRepository
    {
        public SubredditStatisticsModel? GetStatistics(string subreddit);

        public void SetStatistics(string subreddit, SubredditStatisticsModel statistics);
    }
}
