using Newtonsoft.Json;
using RedditTopPostsAndUsers.Models;
using System.Collections.Concurrent;

namespace RedditTopPostsAndUsers.Repositories
{
    public class SubredditStatisticsRepository : ISubredditStatisticsRepository
    {
        private readonly ConcurrentDictionary<string, SubredditStatisticsModel> _statistics = new ConcurrentDictionary<string, SubredditStatisticsModel>();

        public SubredditStatisticsModel? GetSubredditStatistics(string subreddit)
        {
            return _statistics.ContainsKey(subreddit)
                ? _statistics[subreddit]
                : null;
        }

        public void SetSubredditStatistics(string subreddit, SubredditStatisticsModel statistics)
        {
            _statistics[subreddit] = statistics;
        }
    }
}
