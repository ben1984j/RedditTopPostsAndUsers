using RedditTopPostsAndUsers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// builder.Services.AddScoped


var app = builder.Build();

// app.Services.GetService

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

var redditApi = new RedditApi(
    app.Configuration["RedditApiClientId"],
    app.Configuration["RedditApiClientSecret"],
    new SubredditStatisticsRepository()
);

await redditApi.SetAccessToken();

redditApi.MonitorSubreddit("music");

app.Run();
