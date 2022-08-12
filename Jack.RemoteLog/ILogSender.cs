using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jack.RemoteLog
{
    internal interface ILogSender
    {
        void Send(LogItem logitem);
    }
}
