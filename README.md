## Setup

The following variables must be populated in `appsettings.json`:
* `"RedditApiClientId"`
* `"RedditApiClientSecret"`
* `"SubredditsToMonitor"` - a comma-separated list of subreddits, i.e. `"music,funny"`

## Usage

Upon start-up, the application will begin monitoring the specified subreddits.  The console will continuously print out informational messages pertaining to the requests being made to the Reddit API.

To see the statistics that are being compiled from these requests, access the following API endpoint: `GET /api/SubredditStatistics/{subreddit}`.
* _Note: `{subreddit}` must be one of the subreddits that was specified in the `"SubredditsToMonitor"` application setting._

The endpoint will return a JSON response containing information about the top 10 posts and users (or less than 10, if the app has not been running long enough to accumulate that many of each).