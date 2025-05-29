using Jack.RemoteLog;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Extensions.Logging
{
    public static class NormalExtention
    {
        internal class TraceInfo
        {
            public string TraceId;
            public string TraceName;
        }

        /// <summary>
        /// 设置日志追踪信息
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="traceId"></param>
        /// <param name="traceName"></param>
        public static void SetTraceInfo(this ILogger logger, string traceId,string traceName)
        {
            QueueLogger.TracingInfo.Value = new TraceInfo
            {
                TraceId = traceId,
                TraceName = traceName
            };
        }
    }
}
