﻿using Newtonsoft.Json;
using System.Collections.Concurrent;

namespace RedditTopPostsAndUsers
{
    public class SubredditStatisticsRepository : ISubredditStatisticsRepository
    {
        private readonly ConcurrentDictionary<string, SubredditStatisticsModel> _statistics = new ConcurrentDictionary<string, SubredditStatisticsModel>();

        public SubredditStatisticsModel? GetStatistics(string subreddit)
        {
            return _statistics.ContainsKey(subreddit)
                ? _statistics[subreddit]
                : null;
        }

        public void SetStatistics(string subreddit, SubredditStatisticsModel statistics)
        {
            _statistics[subreddit] = statistics;

            //Console.WriteLine(JsonConvert.SerializeObject(statistics?.Posts?.OrderByDescending(x => x.Upvotes).Take(2), Formatting.Indented));
            //Console.WriteLine(JsonConvert.SerializeObject(statistics?.Users?.OrderByDescending(x => x.PostCount).Take(2), Formatting.Indented));
        }
    }
}
