using Jack.RemoteLog.WebApi.Dtos;

namespace Jack.RemoteLog.WebApi.Infrastructures
{
    public interface ILogContentReader
    {
        LogItem[] Read(string sourceContext , long startTimeStamp, long? endTimeStamp, string keyWord);
    }
}
