namespace RedditTopPostsAndUsers
{
    public interface ISubredditStatisticsService
    {
        public Task MonitorSubreddit(string subreddit);
    }
}