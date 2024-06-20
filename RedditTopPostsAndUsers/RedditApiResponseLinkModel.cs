using Newtonsoft.Json;

namespace RedditTopPostsAndUsers
{
    public class RedditApiResponseLinkModel
    {
        public string? Title { get; set; }

        public string? Name { get; set; } // unique ID, used for pagination of subsequent requests

        public string? Author { get; set; }

        public int Score { get; set; }
    }
}
