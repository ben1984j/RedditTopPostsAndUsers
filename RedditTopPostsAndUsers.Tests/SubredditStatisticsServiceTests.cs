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

            var firstPostId = "t3_1dk2fkz";

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
                                        ""name"": """ + firstPostId + @""",
                                        ""author"": ""user1"",
                                    }
                                }
                            ]
                        }
                    }"
                });

            mockRedditApi
                .Setup(x => x.GetNewPosts(subreddit, firstPostId, "100"))
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
                .Setup(x => x.SetSubredditStatistics(subreddit, It.IsAny<SubredditStatisticsModel>()));
            // TODO: could match SubredditStatisticsModel more specifically to see if statistics were computed correctly;
            // however, this would require more extensive sample JSON above

            var subredditStatisticsService = new SubredditStatisticsService(
                mockRedditApi.Object,
                mockSubredditStatisticsRepository.Object
            );

            // Act
            await subredditStatisticsService.MonitorSubreddit(subreddit, maxIterations: 1);

            // Assert
            mockRepository.VerifyAll();
        }
    }
}