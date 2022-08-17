using Jack.RemoteLog.WebApi.Domains;
using Jack.RemoteLog.WebApi.Dtos;

namespace Jack.RemoteLog.WebApi.Infrastructures
{
    public interface ILogContentReader:IDisposable
    {
        LogItem[] Read(ISourceContextCollection sourceContextes, string sourceContext, LogLevel? level, long startTimeStamp, long? endTimeStamp, string keyWord);
    }
}
