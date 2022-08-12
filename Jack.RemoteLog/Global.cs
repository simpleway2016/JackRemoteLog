using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jack.RemoteLog
{
    internal class Global
    {
        public static LogLevel MinimumLevel { get; set; }
        public static IConfiguration Configuration { get; set; }
    }
}
