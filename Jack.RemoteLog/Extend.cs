using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Jack.RemoteLog
{
    public static class Extens
    {

        static ILoggingBuilder LoggingBuilder;
        static void ConfigurationChangeCallback(object p)
        {
            Global.Configuration.GetReloadToken().RegisterChangeCallback(ConfigurationChangeCallback, null);

            string minimumLevel = Global.Configuration["Logging:LogLevel:Default"];
            Global.MinimumLevel = (LogLevel)Enum.Parse<LogLevel>(minimumLevel);
            LoggingBuilder.SetMinimumLevel(Global.MinimumLevel);
        }

        /// <summary>
        /// 使用Jack.RemoteLog异步远程日志
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configuration"></param>
        public static void UseJackRemoteLogger(this ILoggingBuilder builder, IConfiguration configuration,string applicationContext)
        {
            Global.Configuration = configuration;

            LoggingBuilder = builder;
            ConfigurationChangeCallback(null);

            builder.AddProvider(new AsyncLoggerProvider(applicationContext));
        }

        /// <summary>
        /// 使用Jack.RemoteLog异步远程日志
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IHostBuilder UseJackRemoteLogger(this IHostBuilder builder, IConfiguration configuration, string applicationContext)
        {
            Global.Configuration = configuration;
            builder.ConfigureServices((cx,services) =>
            {
                services.AddSingleton<ILoggerFactory>(new AsyncLoggerFactory(applicationContext));
            });
            return builder;
        }

    }
}
