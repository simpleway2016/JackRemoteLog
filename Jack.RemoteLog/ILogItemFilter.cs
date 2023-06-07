using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jack.RemoteLog
{
    /// <summary>
    /// 过滤将要记录的LogItem
    /// </summary>
    public interface ILogItemFilter
    {
        /// <summary>
        /// 将要写入日志
        /// </summary>
        /// <param name="logItem"></param>
        void OnExecuting(LogItem logItem);
    }
}
