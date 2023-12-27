using Jack.RemoteLog;

namespace TestWebApi
{
    public class MyLogFilter : ILogItemFilter
    {
      public static AsyncLocal<string> TrackId { get; set; } = new AsyncLocal<string>();
        public void OnExecuting(LogItem logItem)
        {
            logItem.TraceId = TrackId.Value;
        }
    }
}
