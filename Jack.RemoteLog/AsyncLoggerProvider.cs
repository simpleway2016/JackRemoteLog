using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jack.RemoteLog
{
    class AsyncLoggerProvider : Microsoft.Extensions.Logging.ILoggerProvider
    {
        string _applicationContext;
        public AsyncLoggerProvider(string applicationContext)
        {
            _applicationContext = applicationContext;
        }

        public Microsoft.Extensions.Logging.ILogger CreateLogger(string categoryName)
        {
            return new QueueLogger(_applicationContext,categoryName);
        }

        public void Dispose()
        {

        }
    }
}
