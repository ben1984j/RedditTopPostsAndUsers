﻿using Newtonsoft.Json;
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
        private const string _userAgent = "TopPostsAndUsers v1.0 by u/ben1984j";

        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly Semaphore _apiRequestLock = new Semaphore(1, 1);

        private string? _accessToken = null;

        public RedditApi(string clientId, string clientSecret)
        {
            _clientId = clientId;
            _clientSecret = clientSecret;
        }

        public async Task SetAccessToken()
        {
            // tODO: lock.

            _apiRequestLock.WaitOne();

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

            _apiRequestLock.Release();
        }


        public async Task<RestResponse?> GetNewPosts(string subreddit, string before, string limit)
        {
            // TODO: check if access token null?


            Console.WriteLine($"Getting new posts from subreddit {subreddit} (before = '{before}', limit = '{limit}')");

 
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

                    request.AddQueryParameter("before", before);



                request.AddQueryParameter("limit", limit); // TODO: TEST

                request.AddHeader("User-Agent", _userAgent); // TODO: private var.


                    var response = await restClient.ExecuteAsync(request);


            //if (response.StatusCode == HttpStatusCode.Unauthorized)
            //{
            //    await SetAccessToken(); // likely expired
            //    return response; // don't bother retrying, should work next time
            //}



            decimal.TryParse(response?.GetHeaderValue("x-ratelimit-remaining"), out var rateLimitRemainingRequests);
                decimal.TryParse(response?.GetHeaderValue("x-ratelimit-reset"), out var rateLimitRemainingSecondsUntilReset);

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
                }

            _apiRequestLock.Release(); // TODO: try/finally.

            return response;

        }
    }
}
