using Microsoft.AspNetCore.Mvc;
using Moq;
using RedditTopPostsAndUsers.Controllers;
using RedditTopPostsAndUsers.Models;
using RedditTopPostsAndUsers.Services;

namespace RedditTopPostsAndUsers.Tests
{
    public class SubredditStatisticsControllerTests
    {
        [Test]
        public void GetSubredditStatisticsReturnsOk()
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

            var mockSubredditStatisticsService = new Mock<ISubredditStatisticsService>(MockBehavior.Strict);
            mockSubredditStatisticsService
                .Setup(x => x.GetSubredditStatistics(subreddit))
                .Returns(subredditStatistics);

            var subredditStatisticsController = new SubredditStatisticsController(mockSubredditStatisticsService.Object);

            // Act
            var response = subredditStatisticsController.GetSubredditStatistics(subreddit);

            // Assert
            mockSubredditStatisticsService.VerifyAll();

            var result = response as OkObjectResult;
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Value, Is.EqualTo(subredditStatistics));
        }

        [Test]
        public void GetSubredditStatisticsReturnsNotFound()
        {
            // Arrange
            var subreddit = "invalid";

            var mockSubredditStatisticsService = new Mock<ISubredditStatisticsService>(MockBehavior.Strict);
            mockSubredditStatisticsService
                .Setup(x => x.GetSubredditStatistics(subreddit))
                .Returns((SubredditStatisticsModel?)null);

            var subredditStatisticsController = new SubredditStatisticsController(mockSubredditStatisticsService.Object);

            // Act
            var response = subredditStatisticsController.GetSubredditStatistics(subreddit);

            // Assert
            var result = response as NotFoundResult;
            Assert.That(result, Is.Not.Null);
        }
    }
}