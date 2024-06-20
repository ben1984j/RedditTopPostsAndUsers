namespace RedditTopPostsAndUsers
{
    public class RedditApiResponseListingModel<T>
    {
        public string? Before { get; set; }

        public string? After { get; set; }

        public IEnumerable<T>? Children { get; set; }
    }
}
