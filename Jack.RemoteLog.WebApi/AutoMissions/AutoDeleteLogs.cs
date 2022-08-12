using Quartz;
using System.Text.RegularExpressions;

namespace Jack.RemoteLog.WebApi.AutoMissions
{
    public class AutoDeleteLogs : IJob
    {
        ILogger<AutoDeleteLogs> _logger;
        public AutoDeleteLogs()
        {
            _logger = Global.ServiceProvider.GetService<ILogger<AutoDeleteLogs>>();
        }

        Task IJob.Execute(IJobExecutionContext context)
        {
            return Task.Run(() => {
                try
                {
                    var timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds() - Convert.ToInt32(Global.Configuration["KeepDays"]) * 24 * 60 * 60000;
                    var folder = Global.Configuration["DataPath"];
                    if (Directory.Exists(folder) == false)
                        return;

                    var appFolders = Directory.GetDirectories(folder);
                    foreach( var appfolder in appFolders )
                    {
                        var logfolder = $"{appfolder}/logs";
                        var files = Directory.GetFiles(logfolder, "*.txt");
                        foreach( var filepath in files )
                        {
                            var filename = Path.GetFileName(filepath);
                            var m = Regex.Match(filename, @"[0-9]+");
                            if(m.Length > 0)
                            {
                                if(long.TryParse(m.Value , out long filetime))
                                {
                                    if(filetime < timestamp)
                                    {
                                        //删除文件
                                        try
                                        {
                                            File.Delete(filepath);
                                        }
                                        catch (Exception ex)
                                        {
                                            _logger.LogError(ex, "删除文件异常");
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "");
                }
            });
        }
    }
}
