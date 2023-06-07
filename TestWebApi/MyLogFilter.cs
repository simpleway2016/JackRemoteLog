using Jack.RemoteLog;

namespace TestWebApi
{
    public class MyLogFilter : ILogItemFilter
    {
        public void OnExecuting(LogItem logItem)
        {
            logItem.TraceId = "testTraceId3";
        }
    }
}
