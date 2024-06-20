namespace RedditTopPostsAndUsers
{
    public interface ISubredditStatisticsRepository
    {
        public SubredditStatisticsModel? GetStatistics(string subreddit);

        public void SetStatistics(string subreddit, SubredditStatisticsModel statistics);
    }
}
