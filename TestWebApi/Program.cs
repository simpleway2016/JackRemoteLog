using Jack.RemoteLog;
using TestWebApi;

var builder = WebApplication.CreateBuilder(args);
Console.WriteLine(builder.Configuration["Logging"]);
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<ILogItemFilter, MyLogFilter>();
builder.Services.AddLogging(b =>
{
    b.UseJackRemoteLogger(builder.Configuration , new Options { 
        UserName= "JACK",
        Password= "123"
    });
});
// Add services to the container.

builder.Services.AddControllers();

var app = builder.Build();

var logger = app.Services.GetService<ILogger<Program>>();
logger.LogInformation("≤‚ ‘–≈œ¢");
// Configure the HTTP request pipeline.

app.UseAuthorization();

app.MapControllers();

app.Run();
