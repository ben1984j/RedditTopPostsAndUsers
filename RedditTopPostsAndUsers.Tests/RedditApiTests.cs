using Moq;
using RedditTopPostsAndUsers.ExternalApis;
using RedditTopPostsAndUsers.Models;
using RedditTopPostsAndUsers.Repositories;
using RedditTopPostsAndUsers.Services;
using RestSharp;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net;

namespace RedditTopPostsAndUsers.Tests
{
    public class RedditApiTests
    {
        [Test]
        public async Task GetNewPostsReturnsOkResponse()
        {
            // Arrange
            var subreddit = "funny";
            var before = "post1";
            var limit = "25";

            var rateLimitRemainingRequests = 10m;
            var rateLimitRemainingSecondsUntilReset = 60m;

            var newPostsRestResponse = new RestResponse()
            {
                StatusCode = HttpStatusCode.OK,
                IsSuccessStatusCode = true,
                Content = @"
                {
                    ""kind"": ""Listing"",
                    ""data"": {
                        ""children"": [
                            {
                                ""kind"": ""t3"",
                                ""data"": {
                                    ""name"": ""post2"",
                                    ""author"": ""userB"",
                                    ""ups"": 37
                                }
                            },
                            {
                                ""kind"": ""t3"",
                                ""data"": {
                                    ""name"": ""post1"",
                                    ""author"": ""userA"",
                                    ""ups"": 11
                                }
                            }
                        ]
                    }
                }",
                Headers = new ReadOnlyCollection<HeaderParameter>(
                    new List<HeaderParameter>()
                    {
                        new HeaderParameter("x-ratelimit-remaining", rateLimitRemainingRequests.ToString()),
                        new HeaderParameter("x-ratelimit-reset", rateLimitRemainingSecondsUntilReset.ToString())
                    }
                )
            };

            var mockRepository = new MockRepository(MockBehavior.Strict);

            var mockBaseUrlRestClient = mockRepository.Create<IRestClient>();
            mockBaseUrlRestClient
                .Setup(x =>
                    x.ExecuteAsync(
                        It.Is<RestRequest>(r => r.Resource == "/api/v1/access_token"),
                        default
                    )
                )
                .ReturnsAsync(
                    new RestResponse()
                    {
                        Content = "{\"access_token\": \"12345\"}"
                    });


            var mockOAuthUrlRestClient = mockRepository.Create<IRestClient>();
            mockOAuthUrlRestClient
                .Setup(x =>
                    x.ExecuteAsync(
                        It.Is<RestRequest>(r => 
                            r.Resource == $"/r/{subreddit}/new" &&
                            r.Parameters.FirstOrDefault(p => p.Type == ParameterType.QueryString && p.Name == "before").Value == before &&
                            r.Parameters.FirstOrDefault(p => p.Type == ParameterType.QueryString && p.Name == "limit").Value == limit
                        ),
                        default
                    )
                )
                .ReturnsAsync(newPostsRestResponse);

            var redditApi = new RedditApi(
                mockBaseUrlRestClient.Object,
                mockOAuthUrlRestClient.Object
            );

            // Act
            var stopwatch = Stopwatch.StartNew();
            var response = await redditApi.GetNewPosts(subreddit, before, limit);
            stopwatch.Stop();

            // Assert
            mockRepository.VerifyAll();

            Assert.That(response, Is.EqualTo(newPostsRestResponse));
            Assert.That(stopwatch.ElapsedMilliseconds * 1000, Is.GreaterThanOrEqualTo(rateLimitRemainingSecondsUntilReset / rateLimitRemainingRequests));
        }

        [Test]
        public async Task GetNewPostsReturnsTooManyRequestsResponse()
        {
            // Arrange
            var subreddit = "funny";
            var before = "post1";
            var limit = "25";

            var rateLimitRemainingRequests = 0m;
            var rateLimitRemainingSecondsUntilReset = 15m;

            var newPostsRestResponse = new RestResponse()
            {
                StatusCode = HttpStatusCode.TooManyRequests,
                IsSuccessStatusCode = false,
                Headers = new ReadOnlyCollection<HeaderParameter>(
                    new List<HeaderParameter>()
                    {
                        new HeaderParameter("x-ratelimit-remaining", rateLimitRemainingRequests.ToString()),
                        new HeaderParameter("x-ratelimit-reset", rateLimitRemainingSecondsUntilReset.ToString())
                    }
                )
            };

            var mockRepository = new MockRepository(MockBehavior.Strict);

            var mockBaseUrlRestClient = mockRepository.Create<IRestClient>();
            mockBaseUrlRestClient
                .Setup(x =>
                    x.ExecuteAsync(
                        It.Is<RestRequest>(r => r.Resource == "/api/v1/access_token"),
                        default
                    )
                )
                .ReturnsAsync(
                    new RestResponse()
                    {
                        Content = "{\"access_token\": \"12345\"}"
                    });


            var mockOAuthUrlRestClient = mockRepository.Create<IRestClient>();
            mockOAuthUrlRestClient
                .Setup(x =>
                    x.ExecuteAsync(
                        It.Is<RestRequest>(r =>
                            r.Resource == $"/r/{subreddit}/new" &&
                            r.Parameters.FirstOrDefault(p => p.Type == ParameterType.QueryString && p.Name == "before").Value == before &&
                            r.Parameters.FirstOrDefault(p => p.Type == ParameterType.QueryString && p.Name == "limit").Value == limit
                        ),
                        default
                    )
                )
                .ReturnsAsync(newPostsRestResponse);

            var redditApi = new RedditApi(
                mockBaseUrlRestClient.Object,
                mockOAuthUrlRestClient.Object
            );

            // Act
            var stopwatch = Stopwatch.StartNew();
            var response = await redditApi.GetNewPosts(subreddit, before, limit);
            stopwatch.Stop();

            // Assert
            mockRepository.VerifyAll();

            Assert.That(response, Is.EqualTo(newPostsRestResponse));
            Assert.That(stopwatch.ElapsedMilliseconds * 1000, Is.GreaterThanOrEqualTo(rateLimitRemainingSecondsUntilReset));
        }

        [Test]
        public async Task GetNewPostsReturnsUnauthorizedResponse()
        {
            // Arrange
            var subreddit = "funny";
            var before = "post1";
            var limit = "25";

            var newPostsRestResponse = new RestResponse()
            {
                StatusCode = HttpStatusCode.Unauthorized,
                IsSuccessStatusCode = false
            };

            var mockRepository = new MockRepository(MockBehavior.Strict);

            var mockBaseUrlRestClient = mockRepository.Create<IRestClient>();
            mockBaseUrlRestClient
                .Setup(x =>
                    x.ExecuteAsync(
                        It.Is<RestRequest>(r => r.Resource == "/api/v1/access_token"),
                        default
                    )
                )
                .ReturnsAsync(
                    new RestResponse()
                    {
                        Content = "{\"access_token\": \"invalid\"}"
                    });


            var mockOAuthUrlRestClient = mockRepository.Create<IRestClient>();
            mockOAuthUrlRestClient
                .Setup(x =>
                    x.ExecuteAsync(
                        It.Is<RestRequest>(r =>
                            r.Resource == $"/r/{subreddit}/new" &&
                            r.Parameters.FirstOrDefault(p => p.Type == ParameterType.QueryString && p.Name == "before").Value == before &&
                            r.Parameters.FirstOrDefault(p => p.Type == ParameterType.QueryString && p.Name == "limit").Value == limit
                        ),
                        default
                    )
                )
                .ReturnsAsync(newPostsRestResponse);

            var redditApi = new RedditApi(
                mockBaseUrlRestClient.Object,
                mockOAuthUrlRestClient.Object
            );

            // Act
            var response = await redditApi.GetNewPosts(subreddit, before, limit);

            // Assert
            mockRepository.VerifyAll();

            Assert.That(response, Is.EqualTo(newPostsRestResponse));
            // TODO: private variable _accessToken should have been set to null, but no easy way for test to verify that
        }
    }
}