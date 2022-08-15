using Jack.RemoteLog.WebApi.Dtos;

namespace Jack.RemoteLog.WebApi.Infrastructures
{
    public interface ILogContentWriter:IDisposable
    {
        void Write(WriteLogModel writeLogModel);
    }
}
