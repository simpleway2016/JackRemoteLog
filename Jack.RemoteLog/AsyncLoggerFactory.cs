using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace Jack.RemoteLog
{
    class AsyncLoggerFactory : ILoggerFactory
    {
        ILoggerProvider _loggerProvider;
        public AsyncLoggerFactory(Options options,ILogItemFilter logItemFilter)
        {
            _loggerProvider = new AsyncLoggerProvider(options.ApplicationContext, logItemFilter);
        }
        public void AddProvider(ILoggerProvider provider)
        {
            if(provider == null)
                throw new ArgumentNullException(nameof(provider));

            _loggerProvider = provider;
        }

        public Microsoft.Extensions.Logging.ILogger CreateLogger(string categoryName)
        {
            return _loggerProvider?.CreateLogger(categoryName);
        }

        public void Dispose()
        {

        }
    }


}