using Newtonsoft.Json;
using RestSharp;
using RestSharp.Authenticators;
using RestSharp.Authenticators.OAuth2;
using System.Collections.Concurrent;
using System.Net;

namespace RedditTopPostsAndUsers
{
    public class SubredditStatisticsRepository
    {

        private readonly ConcurrentDictionary<string, SubredditStatisticsModel> _statistics = new ConcurrentDictionary<string, SubredditStatisticsModel>();

        public SubredditStatisticsRepository()
        {

            // var b = new  OAuth2AuthorizationRequestHeaderAuthenticator()
            // {
                
            // }

            // var t = new RestClient(new RestClient(new RestClientOptions(null) { Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator()}))
        }

        public SubredditStatisticsModel? GetStatistics(string subreddit)
        {
            // TODO: if not found?

            return _statistics.ContainsKey(subreddit)
                ? _statistics[subreddit]
                : null;
                //: new SubredditStatisticsModel()
                //{
                //    Posts = new List<SubredditStatisticsPostModel>(),
                //    Users = new List<SubredditStatisticsUserModel>()
                //};

            // tODO: lock.

        }

        public void SetStatistics(string subreddit, SubredditStatisticsModel statistics)
        {
            _statistics[subreddit] = statistics;


            Console.WriteLine(JsonConvert.SerializeObject(statistics?.Posts?.OrderByDescending(x => x.Upvotes).Take(2), Formatting.Indented));
            Console.WriteLine(JsonConvert.SerializeObject(statistics?.Users?.OrderByDescending(x => x.PostCount).Take(2), Formatting.Indented));

        }


    }
}
