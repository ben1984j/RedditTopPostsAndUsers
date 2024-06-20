using RestSharp;

namespace RedditTopPostsAndUsers
{
    public interface IRedditApi
    {
        public Task<RestResponse?> GetNewPosts(string subreddit, string before, string limit);
    }
}
