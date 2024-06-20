namespace RedditTopPostsAndUsers
{
    public class RedditApiResponseModel<T>
    {
        public string? Kind { get; set; }

        public T? Data { get; set; }
    }
}
