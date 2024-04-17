using Jack.RemoteLog.WebApi;
using Jack.RemoteLog.WebApi.Applications;
using Quartz.Impl;
using Quartz;
using Jack.RemoteLog.WebApi.AutoMissions;
using Microsoft.Extensions.FileProviders;
using Quartz.Impl.AdoJobStore.Common;
using System.IO;
using Jack.RemoteLog.WebApi.Controllers;
using JMS.Common;
using Microsoft.Extensions.Primitives;
using System.Text;
using JMS;
using Jack.RemoteLog.WebApi.Dtos;
using Org.BouncyCastle.X509;
using System.Security.Cryptography.X509Certificates;
using System.Reflection.PortableExecutable;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.ResponseCompression;
using System.IO.Compression;

Environment.CurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;
ThreadPool.GetMinThreads(out int w, out int c);
if (w < 500 || c < 500)
{
    ThreadPool.SetMinThreads(500, 500);
}

CommandArgParser cmdArg = new CommandArgParser(args);
var appSettingPath = cmdArg.TryGetValue<string>("-s");

if (appSettingPath == null)
    appSettingPath = "appsettings.json";

if (appSettingPath == "share")
{
    appSettingPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
    appSettingPath = Path.Combine(appSettingPath, "jack.remotelog.webapi");
    if (Directory.Exists(appSettingPath) == false)
    {
        Directory.CreateDirectory(appSettingPath);
    }
    appSettingPath = Path.Combine(appSettingPath, "appsettings.json");
    if (File.Exists(appSettingPath) == false)
    {
        File.Copy("./appsettings.json", appSettingPath);
    }
}


var builder = WebApplication.CreateBuilder(args);
builder.Host.ConfigureAppConfiguration((hostingContext, config) =>
{
    config.AddJsonFile(appSettingPath,
                       optional: true,
                       reloadOnChange: true);
});

var certPath = builder.Configuration["SSL:Cert"];
var pwd = builder.Configuration["SSL:Password"];
if (!string.IsNullOrWhiteSpace(certPath))
{
    // Configure Kestrel server
    builder.WebHost.ConfigureKestrel(serverOptions =>
    {
        // Set up server options here
        var x509ca = new X509Certificate2(certPath, pwd);
        serverOptions.ConfigureEndpointDefaults(c => c.UseHttps(x509ca));
    });
}


builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Optimal;
});

builder.Services.Configure<GzipCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Optimal;
});
builder.Services.AddResponseCompression(options =>
{
    //options.EnableForHttps = true;
    // 添加br与gzip的Provider
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
    // 扩展一些类型 (MimeTypes中有一些基本的类型,可以打断点看看)
    options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[]
    {
                    "text/html; charset=utf-8",
                    "application/xhtml+xml",
                    "application/atom+xml",
                    "image/svg+xml"
                });
});

builder.Services.AddJmsTokenAspNetCore(null, new string[] { "Authorization" });

// Add services to the container.
Global.Configuration = builder.Configuration;

builder.Services.AddControllers();

builder.Services.AddSingleton<LogChannelRoute>();
builder.Services.AddSingleton<LogService>();
builder.Services.Configure<ApiBehaviorOptions>((o) =>
{
    o.SuppressModelStateInvalidFilter = true;
});
builder.Services.AddCors(options =>
{
    options.AddPolicy("abc", builder =>
    {
        //App:CorsOrigins in appsettings.json can contain more than one address with splitted by comma.
        builder
          .SetIsOriginAllowed(_ => true)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();

    });
});

var app = builder.Build();
app.UseResponseCompression();//这句话应该放在最上面，否则不起作用

Global.ServiceProvider = app.Services;
Global.UserInfos = app.Configuration.GetSection("Users").GetNewest<UserInfo[]>();


var logService = app.Services.GetService<LogService>();
var errorUserMarker = new ErrorUserMarker();


app.UseStaticFiles();
//开启index.html
app.UseFileServer();



app.Use((context, next) =>
{
    bool pass = false;
    bool iswriting = context.Request.Path.ToString().Contains("/WriteLog");
    if (iswriting)
    {
        if (Global.UserInfos.Current == null || Global.UserInfos.Current.Any(m => m.Writeable) == false)
        {
            pass = true;
        }
        else if (context.Request.Headers.TryGetValue("Authorization", out StringValues authorization))
        {
            var ip = context.Connection.RemoteIpAddress.ToString();
            if (!errorUserMarker.CheckUserIp(ip))
            {
                context.Response.Headers.Add("WWW-Authenticate", "Basic realm=\"Welcome to Jack.RemoteLog\"");
                context.Response.StatusCode = 401;
                return Task.CompletedTask;
            }

            var base64 = authorization.ToString().Substring(6);
            var name_pwds = Encoding.UTF8.GetString(Convert.FromBase64String(base64)).Split(':');
            var user = Global.UserInfos.Current?.FirstOrDefault(m => string.Equals(name_pwds[0], m.Name, StringComparison.OrdinalIgnoreCase));
            if (user == null || user.Password != name_pwds[1] || (iswriting && user.Writeable == false))
            {
                //不通过身份验证
                if (user == null || user.Password != name_pwds[1])
                {
                    errorUserMarker.Error(ip);
                }
            }
            else
            {
                pass = true;
                errorUserMarker.Clear(ip);
            }
        }

        if (!pass)
        {
            context.Response.Headers.Add("WWW-Authenticate", "Basic realm=\"Welcome to Jack.RemoteLog\"");
            context.Response.StatusCode = 401;
            return Task.CompletedTask;
        }

        if (context.Request.Path.ToString().Contains("/WriteLog"))
        {
            //如果直接请求controller里面post方法，会导致查询变慢，访问的线程越多就越慢，原因不明
            return LogController.HandleWriteLog(context, logService);
        }
    }
    return next();
});

app.UseAuthentication();
app.UseAuthorization();

app.UseCors("abc");
app.MapControllers();





ISchedulerFactory sf = new StdSchedulerFactory();
IScheduler scheduler = sf.GetScheduler().ConfigureAwait(false).GetAwaiter().GetResult();

IJobDetail job = JobBuilder.Create<AutoDeleteLogs>().WithIdentity("job1", "mygroup").Build();

var cronExpression = "0 5 1 * * ?";//每天1点05分执行
ITrigger trigger = TriggerBuilder.Create().StartAt(DateTime.Now.AddSeconds(50)).WithCronSchedule(cronExpression).Build();

scheduler.ScheduleJob(job, trigger);
scheduler.Start();

var logchannelRoute = app.Services.GetService<LogChannelRoute>();

var logger = app.Services.GetService<ILogger<Program>>();
logger.LogInformation("AppSettings Path: {0}", appSettingPath);
logger.LogInformation($"Version:{typeof(Global).Assembly.GetName().Version}");

app.Run();
logchannelRoute.Dispose();