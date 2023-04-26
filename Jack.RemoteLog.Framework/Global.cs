using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Jack.RemoteLog
{
    internal class Global
    {
        public static LogLevel MinimumLevel { get; set; }
        public static IConfiguration Configuration { get; set; }
        public static string ServerUrl { get; set; }
        public static AuthenticationHeaderValue Authorization { get; set; }
    }
}
