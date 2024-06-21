using Newtonsoft.Json;
using RestSharp;
using RestSharp.Authenticators;
using RestSharp.Authenticators.OAuth2;
using System.Net;

namespace RedditTopPostsAndUsers.ExternalApis
{
    public class RedditApi : IRedditApi
    {
        private const string _baseUrl = "https://www.reddit.com";
        private const string _oauthUrl = "https://oauth.reddit.com";
        private const string _userAgent = "TopPostsAndUsers v1.0 by u/ben1984j";

        private readonly IRestClient _baseUrlRestClient;
        private readonly IRestClient _oauthUrlRestClient;

        private readonly Semaphore _apiRequestLock = new Semaphore(1, 1);

        private string? _accessToken = null;

        public RedditApi(string clientId, string clientSecret)
            : this(
                new RestClient(
                    new RestClientOptions(_baseUrl)
                    {
                        Authenticator = new HttpBasicAuthenticator(
                            clientId,
                            clientSecret
                        ),
                        ThrowOnAnyError = true
                    }
                ),
                new RestClient(_oauthUrl)
              )
        {
        }

        public RedditApi(IRestClient baseUrlRestClient, IRestClient oauthUrlRestClient)
        {
            _baseUrlRestClient = baseUrlRestClient;
            _oauthUrlRestClient = oauthUrlRestClient;
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

                var request = new RestRequest(
                    $"/r/{subreddit}/new",
                    Method.Get
                )
                {
                    Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(_accessToken, "Bearer")
                };


                request.AddQueryParameter("before", before);
                request.AddQueryParameter("limit", limit);
                request.AddHeader("User-Agent", _userAgent);

                var response = await _oauthUrlRestClient.ExecuteAsync(request);

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _accessToken = null; // likely expired; set to null so will be refreshed on next request
                    return response;
                }

                decimal.TryParse(response?.GetHeaderValue("x-ratelimit-remaining"), out var rateLimitRemainingRequests);
                decimal.TryParse(response?.GetHeaderValue("x-ratelimit-reset"), out var rateLimitRemainingSecondsUntilReset);

                //Console.WriteLine(rateLimitRemainingRequests);
                //Console.WriteLine(rateLimitRemainingSecondsUntilReset);

                if (response?.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    Console.WriteLine($"Hit request limit; waiting for {rateLimitRemainingSecondsUntilReset} seconds until request limit resets");
                    await Task.Delay((int)(rateLimitRemainingSecondsUntilReset * 1000));
                }
                else if (response?.StatusCode == HttpStatusCode.OK)
                {
                    var avgAllowableIntervalBetweenRequests = rateLimitRemainingSecondsUntilReset / rateLimitRemainingRequests;
                    Console.WriteLine($"Waiting for {avgAllowableIntervalBetweenRequests} seconds to stay within request limit");
                    await Task.Delay((int)(avgAllowableIntervalBetweenRequests * 1000));
                }

                // else caller will have to handle; likely just ignore and try again

                return response;
            }
            finally
            {
                _apiRequestLock.Release();
            }
        }

        private async Task SetAccessToken()
        {
            var request = new RestRequest(
                "/api/v1/access_token",
                Method.Post
            );

            request.AddBody("grant_type=client_credentials", ContentType.FormUrlEncoded);

            var response = await _baseUrlRestClient.ExecuteAsync(request);
            var content = JsonConvert.DeserializeObject<dynamic>(response?.Content ?? "{}"); // could deserialize into a dedicated auth response model, but this is easier since we only need one property
            _accessToken = content?.access_token;

            Console.WriteLine($"Set access token: {_accessToken}");
        }
    }
}
