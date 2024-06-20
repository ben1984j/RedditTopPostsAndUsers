using Newtonsoft.Json;
using RestSharp;
using RestSharp.Authenticators;
using RestSharp.Authenticators.OAuth2;
using System.Net;

namespace RedditTopPostsAndUsers
{
    public class RedditApi
    {
        private const string _baseUrl = "https://www.reddit.com";
        private const string _oauthUrl = "https://oauth.reddit.com";

        private readonly string _clientId;
        private readonly string _clientSecret;

        // private const string _subreddit = "music";

        private string? _accessToken = null;
        private string? _firstPostId = null;
        private decimal _rateLimitRemainingRequests = 0;
        private decimal _rateLimitRemainingSecondsUntilReset = 0;

        private readonly Dictionary<string, int> _posts = new Dictionary<string, int>();

        private readonly Dictionary<string, int> _users = new Dictionary<string, int>();

        private readonly Semaphore _apiRequestLock = new Semaphore(1, 1);

        public RedditApi(string clientId, string clientSecret)
        {
            _clientId = clientId;
            _clientSecret = clientSecret;

            // var b = new  OAuth2AuthorizationRequestHeaderAuthenticator()
            // {
                
            // }

            // var t = new RestClient(new RestClient(new RestClientOptions(null) { Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator()}))
        }

        public async Task SetAccessToken()
        {
            // tODO: lock.

            var restClient = new RestClient(
                new RestClientOptions(_baseUrl)
                {
                    Authenticator = new HttpBasicAuthenticator(
                        _clientId,
                        _clientSecret
                    )
                }
            );

            var request = new RestRequest(
                "/api/v1/access_token",
                Method.Post
            );

            request.AddBody("grant_type=client_credentials", ContentType.FormUrlEncoded);

            var response = await restClient.ExecuteAsync(request);

            var content = JsonConvert.DeserializeObject<dynamic>(response?.Content ?? "{}");

            _accessToken = content?.access_token;

            Console.WriteLine(_accessToken);
        }

        public async Task SetFirstPostId(string subreddit)
        {
            // TODO: this could just be in the other loop.

            // tODO: keep dicts of subreddits?  should i even implement it this way?
            // need a diff. class per subreddit, but keep api shared.
            // need a timer to determine when to refresh token too.  but for now, it's static.

            // there will always be at least 1 recent post, so use that as reference point.

            //using (_apiRequestLock)
            //{
                _apiRequestLock.WaitOne();

                var restClient = new RestClient(
                    new RestClientOptions(_oauthUrl)
                    {
                        Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(_accessToken, "Bearer")
                    }
                );

                var request = new RestRequest(
                    $"/r/{subreddit}/new?limit=1",
                    Method.Get
                );

                request.AddHeader("User-Agent", "TopPostsAndUsers v1.0 by u/ben1984j");

                // request.AddBody("grant_type=client_credentials", ContentType.FormUrlEncoded);

                var response = await restClient.ExecuteAsync(request);

                // Console.WriteLine(response.Content);

                var responseObj = JsonConvert.DeserializeObject<RedditApiResponseModel<RedditApiResponseListingModel<RedditApiResponseModel<RedditApiResponseLinkModel>>>>(response?.Content ?? "{}");


                _firstPostId = responseObj?.Data?.Children?.FirstOrDefault()?.Data?.Name;
            _firstPostId = "t3_1dk1lq3"; // test

                Console.WriteLine(_firstPostId);

                _apiRequestLock.Release();
            //}
        }

        public async Task MonitorSubreddit(string subreddit)
        {
            

            // tODO: keep dicts of subreddits?  should i even implement it this way?
            // need a diff. class per subreddit, but keep api shared.
            // need a timer to determine when to refresh token too.  but for now, it's static.

            //while (_firstPostId == null) // tODO: OR IF.
            //{
                await SetFirstPostId(subreddit);
            //}

            

            while (true)
            {
                await GetSubredditStatistics(subreddit);

                // await Task.Delay(5000); // tODO...reasonable refresh interval.  OR just let built in delay do its thing.
            }
        }

        public async Task GetSubredditStatistics(string subreddit)
        {
            Console.WriteLine($"Getting all posts from subreddit {subreddit} starting after initial post {_firstPostId}");

            // tODO: keep dicts of subreddits?  should i even implement it this way?
            // need a diff. class per subreddit, but keep api shared.
            // need a timer to determine when to refresh token too.  but for now, it's static.

            var count = 0;

            // also use this when returning values to controller.

            //using (_apiRequestLock) // tODO: try/finally...this isn't releasing.
            //{
                _apiRequestLock.WaitOne();

                _posts.Clear(); // yes, b/c we're marching through the whole thing again.
                _users.Clear(); // for this one we don't need historical data but may as well update since we're already marching through.

                string? before = _firstPostId;

                do
                {
                    var restClient = new RestClient(
                        new RestClientOptions(_oauthUrl)
                        {
                            Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(_accessToken, "Bearer")
                        }
                    );

                    var request = new RestRequest(
                        $"/r/{subreddit}/new",
                        Method.Get
                    );

                    request.AddQueryParameter("before", before);



                request.AddQueryParameter("limit", "5"); // TODO: TEST

                request.AddHeader("User-Agent", "TopPostsAndUsers v1.0 by u/ben1984j"); // TODO: private var.

                    // request.AddBody("grant_type=client_credentials", ContentType.FormUrlEncoded);

                    var response = await restClient.ExecuteAsync(request);


                    if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    var a = 2;
                }

                    //TODO: if non success, just ignore.

                    // TODO: if unauthorized/forbidden, setAccessToken, then return.  hard to test though w/o an old one.  use one from postman?

                    // TODO: only do the below if 200 ok.

                    // Console.WriteLine(response.Content);

                    var responseObj = JsonConvert.DeserializeObject<RedditApiResponseModel<RedditApiResponseListingModel<RedditApiResponseModel<RedditApiResponseLinkModel>>>>(response?.Content ?? "{}");

                    // before = responseObj?.Data?.Before;




                    before = responseObj?.Data?.Children?.FirstOrDefault()?.Data?.Name;

                    foreach (var result in responseObj?.Data?.Children ?? Enumerable.Empty<RedditApiResponseModel<RedditApiResponseLinkModel>>())
                    {
                        count++;

                        var title = result?.Data?.Title ?? string.Empty;
                        var author = result?.Data?.Author ?? string.Empty;

                        // Console.WriteLine(title);

                        _posts[title] = result?.Data?.Ups ?? 0; // TODO: actually should be upvotes

                        // _topUsers[author] = result?.Data?.Score ?? 0;

                        // TODO: can't keep adding to top users...need to reset.

                        if (!_users.ContainsKey(author))
                        {
                            _users[author] = 0;
                        }
                        _users[author]++;
                    }

                decimal rateLimitRemainingRequests;
                decimal rateLimitRemainingSecondsUntilReset;

                // TODO: will these be present on 429?
                decimal.TryParse(response?.GetHeaderValue("x-ratelimit-remaining"), out rateLimitRemainingRequests);
                decimal.TryParse(response?.GetHeaderValue("x-ratelimit-reset"), out rateLimitRemainingSecondsUntilReset);

                //Console.WriteLine(response?.GetHeaderValue("x-ratelimit-remaining"));
                //Console.WriteLine(response?.GetHeaderValue("x-ratelimit-reset"));

                if (rateLimitRemainingRequests == 0)
                {
                    Console.WriteLine($"Hit request limit; waiting for {rateLimitRemainingSecondsUntilReset} seconds");

                    await Task.Delay((int)(rateLimitRemainingSecondsUntilReset * 1000));
                }
                else
                {
                    var avgAllowableIntervalBetweenRequests = rateLimitRemainingSecondsUntilReset / rateLimitRemainingRequests;
                    Console.WriteLine($"Waiting for {avgAllowableIntervalBetweenRequests} seconds to stay within request limit");

                    await Task.Delay((int)(avgAllowableIntervalBetweenRequests * 1000));

                    // avgRequestsAllowedPerSecond
                }



                // Console.WriteLine(JsonConvert.SerializeObject(_topPosts, Formatting.Indented));

                // Console.WriteLine(JsonConvert.SerializeObject(_topUsers, Formatting.Indented));

                // var content = JsonConvert.DeserializeObject<dynamic>(response?.Content ?? "{}");

            } while (before != null);

                Console.WriteLine($"Retrieved {count} posts");

                _apiRequestLock.Release();
            //}
        }
    }
}
