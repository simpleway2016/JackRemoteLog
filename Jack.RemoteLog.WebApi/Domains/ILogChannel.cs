using Jack.RemoteLog.WebApi.Dtos;

namespace Jack.RemoteLog.WebApi.Domains
{
    public interface ILogChannel
    {
        string ApplicationContext { get; }
        string[] GetAllSourceContext();
        void WriteLog(WriteLogModel request);
        LogItem[] Read(string sourceContext, long startTimeStamp, long? endTimeStamp, string keyWord);
    }
}
