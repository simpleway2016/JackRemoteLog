using Jack.RemoteLog.WebApi;
using Jack.RemoteLog.WebApi.Applications;
using Quartz.Impl;
using Quartz;
using Jack.RemoteLog.WebApi.AutoMissions;
using Microsoft.Extensions.FileProviders;
using Quartz.Impl.AdoJobStore.Common;
using System.IO;

Environment.CurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
Global.Configuration = builder.Configuration;
builder.Services.AddControllers();

builder.Services.AddSingleton<LogChannelRoute>();
builder.Services.AddSingleton<LogService>();

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

Global.ServiceProvider = app.Services;
// Configure the HTTP request pipeline.


app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(AppDomain.CurrentDomain.BaseDirectory + "wwwroot"),
});

app.UseAuthorization();
app.UseCors("abc");
app.MapControllers();

ISchedulerFactory sf = new StdSchedulerFactory();
IScheduler scheduler = sf.GetScheduler().ConfigureAwait(false).GetAwaiter().GetResult();

IJobDetail job = JobBuilder.Create<AutoDeleteLogs>().WithIdentity("job1", "mygroup").Build();

var cronExpression = "* * * 1/1 * ?";
ITrigger trigger = TriggerBuilder.Create().StartAt(DateTime.Now.AddSeconds(5)).WithCronSchedule(cronExpression).Build();

scheduler.ScheduleJob(job, trigger);
scheduler.Start();

var logchannelRoute = app.Services.GetService<LogChannelRoute>();

app.Run();
logchannelRoute.Dispose();