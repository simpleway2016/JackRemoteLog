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
            var context= _httpContextAccessor.HttpContext;
            if (context == null)
            {
                
            }
            else
            {
                logItem.TraceId = context.Request.Headers.Host;
            }
        }
    }
}
