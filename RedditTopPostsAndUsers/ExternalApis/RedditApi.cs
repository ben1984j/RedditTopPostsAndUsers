using Newtonsoft.Json;
using RestSharp;
using RestSharp.Authenticators;
using RestSharp.Authenticators.OAuth2;

namespace RedditTopPostsAndUsers.ExternalApis
{
    public class RedditApi : IRedditApi
    {
        private const string _baseUrl = "https://www.reddit.com";
        private const string _oauthUrl = "https://oauth.reddit.com";
        private const string _userAgent = "TopPostsAndUsers v1.0 by u/ben1984j";

        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly Semaphore _apiRequestLock = new Semaphore(1, 1);

        private string? _accessToken = null;

        public RedditApi(string clientId, string clientSecret)
        {
            _clientId = clientId;
            _clientSecret = clientSecret;

            // _accessToken = "bad"; // TODO: test this as well as expired token.
        }

        public async Task<RestResponse?> GetNewPosts(string subreddit, string before, string limit)
        {
            Console.WriteLine($"Getting new posts from subreddit {subreddit} (before = '{before}', limit = '{limit}')");

            _apiRequestLock.WaitOne();

            try
            {
                if (_accessToken == null)
                {
                    await SetAccessToken();
                }

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
                request.AddQueryParameter("limit", limit);
                request.AddHeader("User-Agent", _userAgent);

                var response = await restClient.ExecuteAsync(request);

                //if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden)
                //{
                //    _accessToken = null; // likely expired; set to null so will be refreshed next time
                //    return response;
                //}

                decimal.TryParse(response?.GetHeaderValue("x-ratelimit-remaining"), out var rateLimitRemainingRequests);
                decimal.TryParse(response?.GetHeaderValue("x-ratelimit-reset"), out var rateLimitRemainingSecondsUntilReset);

                //Console.WriteLine(rateLimitRemainingRequests);
                //Console.WriteLine(rateLimitRemainingSecondsUntilReset);

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
                }

                return response;
            }
            finally
            {
                _apiRequestLock.Release();
            }
        }


        private async Task SetAccessToken()
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

            // TODO: failure should be show stopper.

            var content = JsonConvert.DeserializeObject<dynamic>(response?.Content ?? "{}");

            _accessToken = content?.access_token;

            Console.WriteLine(_accessToken);
        }

    }
}
