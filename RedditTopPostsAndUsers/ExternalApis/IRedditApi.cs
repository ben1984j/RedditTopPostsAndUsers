using RestSharp;

namespace RedditTopPostsAndUsers.ExternalApis
{
    public interface IRedditApi
    {
        public Task<RestResponse?> GetNewPosts(string subreddit, string before, string limit);
    }
}
