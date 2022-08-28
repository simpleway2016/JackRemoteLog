using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Way.Lib;

namespace Jack.RemoteLog
{
    internal class RemoteLogSender : ILogSender
    {
        public RemoteLogSender()
        {
        }
        public void Send(LogItem logitem)
        {
            var url = Global.ServerUrl;
            if (!string.IsNullOrEmpty(url))
            {
                var serverUrl = $"{url}/Log/WriteLog";
                Way.Lib.HttpClient.PostJson(serverUrl, logitem, 8000);
            }
        }
    }
}
