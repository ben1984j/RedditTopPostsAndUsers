using Moq;
using RedditTopPostsAndUsers.ExternalApis;
using RedditTopPostsAndUsers.Models;
using RedditTopPostsAndUsers.Repositories;
using RedditTopPostsAndUsers.Services;
using RestSharp;
using System.Net;

namespace RedditTopPostsAndUsers.Tests
{
    public class SubredditStatisticsServiceTests
    {
        [Test]
        public void GetSubredditStatisticsReturnsData()
        {
            // Arrange
            var subreddit = "funny";
            var subredditStatistics = new SubredditStatisticsModel()
            {
                Posts = new List<SubredditStatisticsPostModel>()
                {
                    new SubredditStatisticsPostModel()
                    {
                        Title = "test post"
                    }
                },
                Users = new List<SubredditStatisticsUserModel>()
                {
                    new SubredditStatisticsUserModel()
                    {
                        Username = "test user",
                        PostCount = 1
                    }
                }
            };

            var mockRepository = new MockRepository(MockBehavior.Strict);

            var mockSubredditStatisticsRepository = mockRepository.Create<ISubredditStatisticsRepository>();
            mockSubredditStatisticsRepository
                .Setup(x => x.GetSubredditStatistics(subreddit))
                .Returns(subredditStatistics);

            var subredditStatisticsService = new SubredditStatisticsService(
                mockRepository.Create<IRedditApi>().Object,
                mockSubredditStatisticsRepository.Object
            );

            // Act
            var response = subredditStatisticsService.GetSubredditStatistics(subreddit);

            // Assert
            mockRepository.VerifyAll();

            Assert.That(response, Is.EqualTo(subredditStatistics));
        }

        [Test]
        public async Task MonitorSubredditReturns()
        {
            // Arrange
            var subreddit = "funny";
            var redditApiPageSize = 2;

            var mockRepository = new MockRepository(MockBehavior.Strict);

            var mockRedditApi = mockRepository.Create<IRedditApi>();
            mockRedditApi
                .Setup(x => x.GetNewPosts(subreddit, "", "1"))
                .ReturnsAsync(new RestResponse()
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
                                        ""name"": ""post0""
                                    }
                                }
                            ]
                        }
                    }"
                });

            mockRedditApi
                .SetupSequence(x => x.GetNewPosts(subreddit, "post0", redditApiPageSize.ToString()))
                .ThrowsAsync(new Exception("Internal server error")) // simulate transient error; monitoring should not terminate but simply move to the next iteration of the monitoring loop
                .ReturnsAsync(new RestResponse()
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
                    }"
                });

            mockRedditApi
                .Setup(x => x.GetNewPosts(subreddit, "post2", redditApiPageSize.ToString()))
                .ReturnsAsync(new RestResponse()
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
                                        ""name"": ""post4"",
                                        ""author"": ""userB"",
                                        ""ups"": 10002
                                    }
                                },
                                {
                                    ""kind"": ""t3"",
                                    ""data"": {
                                        ""name"": ""post3"",
                                        ""author"": ""userC"",
                                        ""ups"": 4
                                    }
                                }
                            ]
                        }
                    }"
                });

            mockRedditApi
                .Setup(x => x.GetNewPosts(subreddit, "post4", redditApiPageSize.ToString()))
                .ReturnsAsync(new RestResponse()
                {
                    StatusCode = HttpStatusCode.OK,
                    IsSuccessStatusCode = true,
                    Content = @"
                    {
                        ""kind"": ""Listing"",
                        ""data"": {
                            ""children"": [
                            ]
                        }
                    }"
                });

            var mockSubredditStatisticsRepository = mockRepository.Create<ISubredditStatisticsRepository>();
            mockSubredditStatisticsRepository
                .Setup(x => x.SetSubredditStatistics(
                    subreddit,
                    It.Is<SubredditStatisticsModel>(y => y.Posts.Count == 4 && y.Users.Count == 3)
                 ));

            var subredditStatisticsService = new SubredditStatisticsService(
                mockRedditApi.Object,
                mockSubredditStatisticsRepository.Object,
                redditApiPageSize
            );

            // Act
            await subredditStatisticsService.MonitorSubreddit(subreddit, maxIterations: 2);

            // Assert
            mockRepository.VerifyAll();
        }

        [Test]
        public void MonitorSubredditThrowsWhenFirstPostNotFound()
        {
            // Arrange
            var subreddit = "funny";

            var mockRepository = new MockRepository(MockBehavior.Strict);

            var mockRedditApi = mockRepository.Create<IRedditApi>();
            mockRedditApi
                .Setup(x => x.GetNewPosts(subreddit, "", "1"))
                .ReturnsAsync(new RestResponse()
                {
                    StatusCode = HttpStatusCode.OK,
                    IsSuccessStatusCode = true,
                    Content = @"
                    {
                        ""kind"": ""Listing"",
                        ""data"": {
                            ""children"": [
                            ]
                        }
                    }"
                });

            var subredditStatisticsService = new SubredditStatisticsService(
                mockRedditApi.Object,
                mockRepository.Create<ISubredditStatisticsRepository>().Object,
                25
            );

            // Act
            var responseException = Assert.ThrowsAsync<Exception>(async () =>
                await subredditStatisticsService.MonitorSubreddit(subreddit, maxIterations: 1)
            );
            

            // Assert
            mockRepository.VerifyAll();

            Assert.That(responseException?.Message, Is.EqualTo($"First post not found for subreddit '{subreddit}'"));
        }
    }
}