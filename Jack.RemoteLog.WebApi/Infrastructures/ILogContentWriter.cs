using Jack.RemoteLog.WebApi.Dtos;

namespace Jack.RemoteLog.WebApi.Infrastructures
{
    public interface ILogContentWriter
    {
        void Write(WriteLogModel writeLogModel);
    }
}
