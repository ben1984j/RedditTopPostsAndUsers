using Newtonsoft.Json;
using RestSharp;
using System.Net;

namespace RedditTopPostsAndUsers
{
    public class SubredditStatisticsService : ISubredditStatisticsService
    {
        private readonly IRedditApi _redditApi;
        private readonly ISubredditStatisticsRepository _subredditStatisticsRepository;

        public SubredditStatisticsService(IRedditApi redditApi, ISubredditStatisticsRepository subredditStatisticsRepository)
        {
            _redditApi = redditApi;
            _subredditStatisticsRepository = subredditStatisticsRepository;
        }

        public async Task MonitorSubreddit(string subreddit)
        {
            // TODO: test invalid subreddit name.

            var firstPostId = await GetFirstPostId(subreddit) ?? throw new Exception("Error retrieving initial post from subreddit.");
            // tODO: handle if null.  can't continue.

            while (true)
            {
                await SetSubredditStatistics(subreddit, firstPostId);
            }
        }

        private async Task<string?> GetFirstPostId(string subreddit)
        {
            var response = await _redditApi.GetNewPosts(subreddit, before: "", limit: "1");

            // TODO: handle non oK.

            var responseObj = JsonConvert.DeserializeObject<RedditApiResponseModel<RedditApiResponseListingModel<RedditApiResponseModel<RedditApiResponseLinkModel>>>>(response?.Content ?? "{}");

            return responseObj?.Data?.Children?.FirstOrDefault()?.Data?.Name;
        }

        private async Task SetSubredditStatistics(string subreddit, string firstPostId)
            {
                Console.WriteLine($"Getting all posts from subreddit {subreddit} starting after initial post {firstPostId}");

                var count = 0;

                string? before = firstPostId;

                var statistics = new SubredditStatisticsModel()
                {
                    Posts = new List<SubredditStatisticsPostModel>(),
                    Users = new List<SubredditStatisticsUserModel>()
                };

                do
                {
                    var response = await _redditApi.GetNewPosts(subreddit, before: before, limit: "100");

                    if (response?.StatusCode != HttpStatusCode.OK)
                    {
                        // tODO: if unauthorized, refresh token and let it go next time.
                        return;
                        // TODO: log
                    }


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

                Console.WriteLine($"Retrieved {count} posts");

                _subredditStatisticsRepository.SetStatistics(subreddit, statistics);
            }
        }
    }
