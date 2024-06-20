namespace RedditTopPostsAndUsers.Models
{
    public class SubredditStatisticsModel
    {
        public IList<SubredditStatisticsPostModel>? Posts { get; set; }

        public IList<SubredditStatisticsUserModel>? Users { get; set; }
    }
}
