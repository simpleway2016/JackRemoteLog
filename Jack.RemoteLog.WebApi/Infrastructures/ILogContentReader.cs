using Jack.RemoteLog.WebApi.Dtos;

namespace Jack.RemoteLog.WebApi.Infrastructures
{
    public interface ILogContentReader:IDisposable
    {
        LogItem[] Read(string sourceContext, LogLevel? level, long startTimeStamp, long? endTimeStamp, string keyWord);
    }
}
