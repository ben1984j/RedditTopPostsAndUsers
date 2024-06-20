namespace RedditTopPostsAndUsers.Services
{
    public interface ISubredditMonitoringService
    {
        public Task MonitorSubreddit(string subreddit);
    }
}