using Jack.RemoteLog.WebApi.Dtos;

namespace Jack.RemoteLog.WebApi.Infrastructures
{
    public interface ILogContentReader:IDisposable
    {
        LogItem[] Read(ISourceContextCollection sourceContextes, SearchRequestBody body);
    }
}
