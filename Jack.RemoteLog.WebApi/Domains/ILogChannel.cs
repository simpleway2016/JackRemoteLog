using Jack.RemoteLog.WebApi.Dtos;

namespace Jack.RemoteLog.WebApi.Domains
{
    public interface ILogChannel: IDisposable
    {
        string ApplicationContext { get; }
        string[] GetAllSourceContext();
        void WriteLog(WriteLogModel request);
        LogItem[] Read(string sourceContext, LogLevel? level, long startTimeStamp, long? endTimeStamp, string keyWord);
    }
}
