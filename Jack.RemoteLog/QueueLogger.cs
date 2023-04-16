using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace Jack.RemoteLog
{
    public class LogItem
    {
        public string ApplicationContext { get; set; }
        public string SourceContext { get; set; }
        public long Timestamp { get; set; }
        public LogLevel Level { get; set; }
        public string Content { get; set; }
    }

    class QueueLogger : Microsoft.Extensions.Logging.ILogger
    {
        string _applicationContext;

        static ConcurrentQueue<LogItem> Queue = new ConcurrentQueue<LogItem>();
        static AutoResetEvent WaitEvent = new AutoResetEvent(false);
        string _categoryName;

        static QueueLogger()
        {
            var sender = new RemoteLogSender();
            new Thread(async () =>
            {
                var handler = new HttpClientHandler();
                handler.ServerCertificateCustomValidationCallback = delegate { return true; };
                using var httpClient = new HttpClient(handler);
                while (true)
                {
                    if (Queue.TryDequeue(out LogItem item))
                    {
                       while(true)
                        {
                            try
                            {
                                await sender.Send(item, httpClient);
                                break;
                            }
                            catch
                            {
                                //保持堆积不超过1000
                                while(Queue.Count > 1000)
                                {
                                    Queue.TryDequeue(out item);
                                }
                                Thread.Sleep(3000);
                            }
                        }
                    }
                    else
                    {
                        WaitEvent.WaitOne();
                    }
                }
            }).Start();
        }

        public QueueLogger(string applicationContext, string categoryName)
        {
            this._applicationContext = applicationContext;
            this._categoryName = categoryName;

        }
        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
            return logLevel >= Global.MinimumLevel;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            //if (IsEnabled(logLevel))
            {
                string msg = state.ToString();
                if (exception != null)
                {
                    if (string.IsNullOrEmpty(msg))
                        msg = exception.ToString();
                    else
                        msg = $"{msg}\r\n{exception.ToString()}";
                }
               
                Queue.Enqueue(new LogItem
                {
                    Level = logLevel,
                    Content = msg,
                    Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    SourceContext = _categoryName,
                    ApplicationContext = _applicationContext
                });
                WaitEvent.Set();
            }
        }
    }

}