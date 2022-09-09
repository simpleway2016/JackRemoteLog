using Jack.RemoteLog.WebApi.Domains;
using System.Collections.Concurrent;

namespace Jack.RemoteLog.WebApi.Applications
{
    public class LogChannelRoute : IDisposable
    {
        ConcurrentDictionary<string, ILogChannel> _dict = new ConcurrentDictionary<string, ILogChannel>();
        public ILogChannel this[string applicationContext]
        {
            get
            {
                return _dict.GetOrAdd(applicationContext, key => {
                    return new FileLogChannel(key);
                });
            }
        }

        public LogChannelRoute()
        {
            var folderPath = Global.Configuration["DataPath"];
            if (Directory.Exists(folderPath) == false)
                Directory.CreateDirectory(folderPath);

            var appFolder = Directory.GetDirectories(folderPath);
            foreach( var folder in appFolder )
            {
                if(File.Exists(folder + "/sourcecontext.db"))
                {
                    var obj = this[Path.GetFileName(folder)];
                }
            }
        }

        public void DeleteLogs(long endTime)
        {
            foreach( var pair in _dict)
            {
                this[pair.Key].DeleteLogs(endTime);
            }
        }


        public string[] GetApplications()
        {
            return _dict.Keys.ToArray();
        }

        public void Dispose()
        {
            var channels = _dict.Values.ToArray();
            _dict.Clear();
            foreach( var channel in channels )
            {
                channel.Dispose();
            }
        }
    }
}
