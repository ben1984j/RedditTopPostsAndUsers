using Newtonsoft.Json;
using RestSharp;
using RestSharp.Authenticators;
using RestSharp.Authenticators.OAuth2;

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

        private readonly Dictionary<string, int> _topPosts = new Dictionary<string, int>();

        private readonly Dictionary<string, int> _topUsers = new Dictionary<string, int>();

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

        public async Task MonitorSubreddit(string subreddit)
        {
            // tODO: keep dicts of subreddits?  should i even implement it this way?
            // need a diff. class per subreddit, but keep api shared.
            // need a timer to determine when to refresh token too.  but for now, it's static.

            using (_apiRequestLock)
            {
                _apiRequestLock.WaitOne();

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

                request.AddHeader("User-Agent", "TopPostsAndUsers v1.0 by u/ben1984j");

                // request.AddBody("grant_type=client_credentials", ContentType.FormUrlEncoded);

                var response = await restClient.ExecuteAsync(request);

                // Console.WriteLine(response.Content);

                var responseObj = JsonConvert.DeserializeObject<RedditApiResponseModel<RedditApiResponseListingModel<RedditApiResponseModel<RedditApiResponseLinkModel>>>>(response?.Content ?? "{}");

                foreach (var result in responseObj?.Data?.Children ?? Enumerable.Empty<RedditApiResponseModel<RedditApiResponseLinkModel>>())
                {
                    var title = result?.Data?.Title ?? string.Empty;
                    var author = result?.Data?.Author ?? string.Empty;

                    Console.WriteLine(title);

                    _topPosts[title] = result?.Data?.Score ?? 0; // TODO: actually should be upvotes

                    // _topUsers[author] = result?.Data?.Score ?? 0;

                    if (!_topUsers.ContainsKey(author))
                    {
                        _topUsers[author] = 0;
                    }

                    _topUsers[author]++;
                }

                Console.WriteLine(JsonConvert.SerializeObject(_topPosts, Formatting.Indented));

                Console.WriteLine(JsonConvert.SerializeObject(_topUsers, Formatting.Indented));

                // var content = JsonConvert.DeserializeObject<dynamic>(response?.Content ?? "{}");
            }
        }
    }
}
