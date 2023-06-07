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
        public AsyncLoggerProvider(Options options, ILogItemFilter logItemFilter)
        {
            this._logItemFilter = logItemFilter;
            _applicationContext = options.ApplicationContext;
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
