using Jack.RemoteLog.WebApi.Domains;
using System.Collections.Concurrent;

namespace Jack.RemoteLog.WebApi.Applications
{
    public class LogChannelRoute
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
            var appFolder = Directory.GetDirectories(folderPath);
            foreach( var folder in appFolder )
            {
                if(File.Exists(folder + "/sourcecontext.txt"))
                {
                    var obj = this[Path.GetFileName(folder)];
                }
            }
        }

        public string[] GetApplications()
        {
            return _dict.Keys.ToArray();
        }
    }
}
