using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jack.RemoteLog
{
    public class Options
    {
        /// <summary>
        /// 日志过滤器，用于过滤将要记录的LogItem
        /// </summary>
        public ILogItemFilter LogItemFilter { get; set; } 
        public string ApplicationContext { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}
