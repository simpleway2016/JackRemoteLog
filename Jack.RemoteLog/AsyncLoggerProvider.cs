using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jack.RemoteLog
{
    class AsyncLoggerProvider : Microsoft.Extensions.Logging.ILoggerProvider
    {
        ILogItemFilter _logItemFilter;
        string _applicationContext;
        public AsyncLoggerProvider(string applicationContext, ILogItemFilter logItemFilter)
        {
            this._logItemFilter = logItemFilter;
            _applicationContext = applicationContext;
        }

        public Microsoft.Extensions.Logging.ILogger CreateLogger(string categoryName)
        {
            return new QueueLogger(_applicationContext,categoryName,_logItemFilter);
        }

        public void Dispose()
        {

        }
    }
}
