using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace Jack.RemoteLog
{
    class AsyncLoggerFactory : ILoggerFactory
    {
        ILoggerProvider _loggerProvider;
        public AsyncLoggerFactory(string applicationContext)
        {
            _loggerProvider = new AsyncLoggerProvider(applicationContext);
        }
        public void AddProvider(ILoggerProvider provider)
        {

        }

        public Microsoft.Extensions.Logging.ILogger CreateLogger(string categoryName)
        {
            return _loggerProvider.CreateLogger(categoryName);
        }

        public void Dispose()
        {

        }
    }


}