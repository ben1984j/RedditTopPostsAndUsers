using RedditTopPostsAndUsers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddSingleton<IRedditApi, RedditApi>(sp => new RedditApi(
    builder.Configuration["RedditApiClientId"],
    builder.Configuration["RedditApiClientSecret"]
));
builder.Services.AddSingleton<ISubredditStatisticsRepository, SubredditStatisticsRepository>();
builder.Services.AddSingleton<ISubredditStatisticsService, SubredditStatisticsService>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

var subredditStatisticsService = app.Services.GetService<ISubredditStatisticsService>();
foreach (var subreddit in app.Configuration["SubredditsToMonitor"]?.Split(',') ?? new string[0])
{
    subredditStatisticsService.MonitorSubreddit(subreddit);
}


app.Run();
