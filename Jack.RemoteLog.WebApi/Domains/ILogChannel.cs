using Jack.RemoteLog.WebApi.Dtos;

namespace Jack.RemoteLog.WebApi.Domains
{
    public interface ILogChannel: IDisposable
    {
        string ApplicationContext { get; }
        string[] GetAllSourceContext();
        /// <summary>
        /// 删除指定时间之前的日志
        /// </summary>
        /// <param name="endTime"></param>
        void DeleteLogs(long endTime);
        void WriteLog(WriteLogModel request);
        LogItem[] Read(string sourceContext, LogLevel? level, long startTimeStamp, long? endTimeStamp, string keyWord);
    }
}
