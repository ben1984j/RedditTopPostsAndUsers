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

        private const string _subreddit = "music";

        private string? _accessToken = null;

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

            var response = await restClient.ExecutePostAsync(request);

            Console.WriteLine(response.Content);
        }
    }
}
