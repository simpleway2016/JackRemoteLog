using Jack.RemoteLog;

var builder = WebApplication.CreateBuilder(args);
Console.WriteLine(builder.Configuration["Logging"]);
builder.Services.AddLogging(b =>
{
    b.UseJackRemoteLogger(builder.Configuration);
});
// Add services to the container.

builder.Services.AddControllers();

var app = builder.Build();

var logger = app.Services.GetService<ILogger<Program>>();
logger.LogDebug("abccc");
// Configure the HTTP request pipeline.

app.UseAuthorization();

app.MapControllers();

app.Run();
