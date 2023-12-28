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

        public string[] GetSourceContexts(string applicationContext)
        {
            return _logChannelRoute[applicationContext].GetAllSourceContext();
        }

        public void WriteLog(WriteLogModel request)
        {
            _logChannelRoute[request.ApplicationContext].WriteLog(request);
        }

        public LogItem[] ReadLogs(SearchRequestBody body)
        {
            return _logChannelRoute[body.AppContext].Read(body);
        }

        public string[] GetApplications()
        {
            return _logChannelRoute.GetApplications();
        }
    }
}
