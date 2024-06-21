using RedditTopPostsAndUsers.Models;

namespace RedditTopPostsAndUsers.Repositories
{
    public interface ISubredditStatisticsRepository
    {
        public SubredditStatisticsModel? GetSubredditStatistics(string subreddit);

        public void SetSubredditStatistics(string subreddit, SubredditStatisticsModel statistics);
    }
}
