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
        //static void ConfigurationChangeCallback(object p)
        //{
        //    ILoggingBuilder loggingBuilder = (ILoggingBuilder)p;
        //    Global.Configuration.GetReloadToken().RegisterChangeCallback(ConfigurationChangeCallback, p);

        //    string minimumLevel = Global.Configuration["Logging:LogLevel:Default"];
        //    if (Enum.TryParse<LogLevel>(minimumLevel , out LogLevel level))
        //    {
        //        Global.MinimumLevel = level;
        //        loggingBuilder.SetMinimumLevel(Global.MinimumLevel);
        //    }
        //    else
        //    {
        //        throw new Exception("配置信息Logging:LogLevel:Default无法转换为LogLevel");
        //    }
        //}

        /// <summary>
        /// 使用Jack.RemoteLog异步远程日志
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configuration"></param>
        public static void UseJackRemoteLogger(this ILoggingBuilder builder, IConfiguration configuration,string applicationContext)
        {
            Global.Configuration = configuration;

            //ConfigurationChangeCallback(builder);

            builder.AddProvider(new AsyncLoggerProvider(applicationContext));
        }

        /// <summary>
        /// 使用Jack.RemoteLog异步远程日志
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        //public static IHostBuilder UseJackRemoteLogger(this IHostBuilder builder, IConfiguration configuration, string applicationContext)
        //{
        //    Global.Configuration = configuration;
        //    builder.ConfigureServices((cx,services) =>
        //    {
        //        services.AddSingleton<ILoggerFactory>(new AsyncLoggerFactory(applicationContext));
        //    });
        //    return builder;
        //}

    }
}
