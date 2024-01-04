using Jack.RemoteLog;
using TestWebApi;

var builder = WebApplication.CreateBuilder(args);
Console.WriteLine(builder.Configuration["Logging"]);

ConfigurationBuilder builder2 = new ConfigurationBuilder();
builder2.AddJsonFile("appsettings2.json", optional: true, reloadOnChange: true);
var configuration2 =  builder2.Build();

var myServices = new ServiceCollection();
myServices.AddLogging(b =>
{
    b.UseJackRemoteLogger(configuration2, new Options
    {
        UserName = "JACK",
        Password = "123"
    });
});
myServices.AddSingleton<ILogItemFilter, MyLogFilter>();
var serviceProvider2 = myServices.BuildServiceProvider();

builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<ILogItemFilter,MyLogFilter> ();
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

app.UseAuthorization();

app.MapControllers();

app.Use((c, next) => {
    MyLogFilter.TrackId.Value = Guid.NewGuid().ToString();
    c.RequestServices.GetService<ILogger<Program>>().LogWarning($"警告-{c.Request.Path}");
    c.RequestServices.GetService<ILogger<Program>>().LogInformation($"有访问-{c.Request.Path}");
    serviceProvider2.GetService<ILogger<Program>>().LogError($"有访问-{c.Request.Host}");

    MyLogFilter.TrackId.Value = null;
    return next();
});

app.Run();
