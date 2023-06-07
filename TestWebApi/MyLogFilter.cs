using Jack.RemoteLog;

namespace TestWebApi
{
    public class MyLogFilter : ILogItemFilter
    {
        IHttpContextAccessor _httpContextAccessor;
        public MyLogFilter(IHttpContextAccessor httpContextAccessor)
        {
            this._httpContextAccessor = httpContextAccessor;

        }
        public void OnExecuting(LogItem logItem)
        {
            logItem.TraceId = "testTraceId5";
        }
    }
}
