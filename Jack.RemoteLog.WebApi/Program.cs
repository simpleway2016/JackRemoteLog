using Jack.RemoteLog.WebApi;
using Jack.RemoteLog.WebApi.Applications;
using Quartz.Impl;
using Quartz;
using Jack.RemoteLog.WebApi.AutoMissions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
Global.Configuration = builder.Configuration;
builder.Services.AddControllers();

builder.Services.AddSingleton<LogChannelRoute>();
builder.Services.AddSingleton<LogService>();

var app = builder.Build();

Global.ServiceProvider = app.Services;
// Configure the HTTP request pipeline.

app.UseAuthorization();

app.MapControllers();

ISchedulerFactory sf = new StdSchedulerFactory();
IScheduler scheduler = sf.GetScheduler().ConfigureAwait(false).GetAwaiter().GetResult();

IJobDetail job = JobBuilder.Create<AutoDeleteLogs>().WithIdentity("job1", "mygroup").Build();

var cronExpression = "* * * 1/1 * ?";
ITrigger trigger = TriggerBuilder.Create().StartAt(DateTime.Now.AddSeconds(5)).WithCronSchedule(cronExpression).Build();

scheduler.ScheduleJob(job, trigger);
scheduler.Start();


app.Run();
