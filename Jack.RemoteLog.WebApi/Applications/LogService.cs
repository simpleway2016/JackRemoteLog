using Jack.RemoteLog.WebApi.Dtos;

namespace Jack.RemoteLog.WebApi.Applications
{
    public class LogService
    {
        LogChannelRoute _logChannelRoute;
        public LogService(LogChannelRoute logChannelRoute)
        {
            this._logChannelRoute = logChannelRoute;

        }

        public string[] GetSourceContextes(string applicationContext)
        {
            return _logChannelRoute[applicationContext].GetAllSourceContext();
        }

        public void WriteLog(WriteLogModel request)
        {
            _logChannelRoute[request.ApplicationContext].WriteLog(request);
        }

        public LogItem[] ReadLogs(string applicationContext, string sourceContext, LogLevel? level, long startTimeStamp, long? endTimeStamp, string keyWord, string? traceId)
        {
            return _logChannelRoute[applicationContext].Read(sourceContext,level, startTimeStamp, endTimeStamp, keyWord, traceId);
        }

        public string[] GetApplications()
        {
            return _logChannelRoute.GetApplications();
        }
    }
}
