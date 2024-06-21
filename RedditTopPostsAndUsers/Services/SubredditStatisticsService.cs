using Newtonsoft.Json;
using RedditTopPostsAndUsers.ExternalApis;
using RedditTopPostsAndUsers.Models;
using RedditTopPostsAndUsers.Repositories;

namespace RedditTopPostsAndUsers.Services
{
    public class SubredditStatisticsService : ISubredditStatisticsService
    {
        private readonly IRedditApi _redditApi;
        private readonly ISubredditStatisticsRepository _subredditStatisticsRepository;
        private readonly int _redditApiPageSize;

        public SubredditStatisticsService(
            IRedditApi redditApi,
            ISubredditStatisticsRepository subredditStatisticsRepository,
            int redditApiPageSize = 100
        )
        {
            _redditApi = redditApi;
            _subredditStatisticsRepository = subredditStatisticsRepository;
            _redditApiPageSize = redditApiPageSize;
        }

        public SubredditStatisticsModel? GetSubredditStatistics(string subreddit) => _subredditStatisticsRepository.GetSubredditStatistics(subreddit);

        public async Task MonitorSubreddit(string subreddit, int? maxIterations = null)
        {
            var firstPostId = await GetFirstPostId(subreddit); // if this throws an error, monitoring will not proceed

            var i = 0;
            while (maxIterations == null || i < maxIterations)
            {
                try
                {
                    await SetSubredditStatistics(subreddit, firstPostId);

                }
                catch (Exception ex)
                {
                    // Log and swallow error, then continue attempting to monitor
                    Console.WriteLine($"Exception encountered while monitoring subreddit '{subreddit}': {ex}");
                }

                i++;
            }
        }

        private async Task<string> GetFirstPostId(string subreddit)
        {
            var response = await _redditApi.GetNewPosts(subreddit, before: "", limit: "1");

            // If non-OK response, this will throw and bubble up to caller
            var responseObj = JsonConvert.DeserializeObject<RedditApiResponseModel<RedditApiResponseListingModel<RedditApiResponseModel<RedditApiResponseLinkModel>>>>(response?.Content ?? "{}");

            return responseObj?.Data?.Children?.FirstOrDefault()?.Data?.Name ?? throw new Exception($"First post not found for subreddit '{subreddit}'");
        }

        private async Task SetSubredditStatistics(string subreddit, string firstPostId)
        {
            Console.WriteLine($"Getting all posts from subreddit '{subreddit}' starting after initial post '{firstPostId}'");

            var count = 0;

            string? before = firstPostId;

            var statistics = new SubredditStatisticsModel()
            {
                Posts = new List<SubredditStatisticsPostModel>(),
                Users = new List<SubredditStatisticsUserModel>()
            };

            do
            {
                var response = await _redditApi.GetNewPosts(subreddit, before: before, limit: _redditApiPageSize.ToString());

                // Assume OK response; if not, this line will throw
                var responseObj = JsonConvert.DeserializeObject<RedditApiResponseModel<RedditApiResponseListingModel<RedditApiResponseModel<RedditApiResponseLinkModel>>>>(response?.Content ?? "{}");

                before = responseObj?.Data?.Children?.FirstOrDefault()?.Data?.Name;

                foreach (var result in responseObj?.Data?.Children ?? Enumerable.Empty<RedditApiResponseModel<RedditApiResponseLinkModel>>())
                {
                    count++;

                    statistics.Posts.Add(new SubredditStatisticsPostModel()
                    {
                        Title = result?.Data?.Title,
                        Url = result?.Data?.Permalink,
                        Upvotes = result?.Data?.Ups ?? 0
                    });

                    var author = result?.Data?.Author;
                    var user = statistics.Users.FirstOrDefault(x => x.Username == author);
                    if (user == null)
                    {
                        user = new SubredditStatisticsUserModel()
                        {
                            Username = author,
                            PostCount = 0
                        };
                        statistics.Users.Add(user);
                    }
                    user.PostCount++;
                }

            } while (before != null);

            Console.WriteLine($"Retrieved {count} posts from subreddit '{subreddit}' starting after initial post '{firstPostId}'");
            Console.WriteLine();

            _subredditStatisticsRepository.SetSubredditStatistics(subreddit, statistics);
        }
    }
}
