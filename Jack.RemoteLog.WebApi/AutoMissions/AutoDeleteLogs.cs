using Jack.RemoteLog.WebApi.Applications;
using Quartz;
using System.Text.RegularExpressions;

namespace Jack.RemoteLog.WebApi.AutoMissions
{
    public class AutoDeleteLogs : IJob
    {
        ILogger<AutoDeleteLogs> _logger;
        LogChannelRoute _logChannelRoute;
        public AutoDeleteLogs()
        {
            _logger = Global.ServiceProvider.GetService<ILogger<AutoDeleteLogs>>();
            _logChannelRoute = Global.ServiceProvider.GetService<LogChannelRoute>();
        }

        Task IJob.Execute(IJobExecutionContext context)
        {
            _logger.LogInformation("AutoDeleteLogs执行");
            return Task.Run(() => {
                try
                {
                    var timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds() - Convert.ToInt32(Global.Configuration["KeepDays"]) * 24 * 60 * 60000;
                    _logChannelRoute.DeleteLogs(timestamp);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "");
                }
            });
        }
    }
}
