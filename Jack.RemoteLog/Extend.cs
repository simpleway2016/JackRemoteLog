﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Jack.RemoteLog
{
    public static class Extens
    {
        static void ConfigurationChangeCallback(object p)
        {
            Global.ServerUrl = Global.Configuration["Logging:ServerUrl"];
            ILoggingBuilder loggingBuilder = (ILoggingBuilder)p;
            Global.Configuration.GetReloadToken().RegisterChangeCallback(ConfigurationChangeCallback, p);

            string minimumLevel = Global.Configuration["Logging:LogLevel:Default"];
            if (Enum.TryParse<LogLevel>(minimumLevel, out LogLevel level))
            {
                Global.MinimumLevel = level;
                loggingBuilder.SetMinimumLevel(Global.MinimumLevel);
            }
            else
            {
                throw new Exception("配置信息Logging:LogLevel:Default无法转换为LogLevel");
            }
        }


        public static void UseJackRemoteLogger(this ILoggingBuilder builder, IConfiguration configuration)
        {
            Global.Configuration = configuration;

            ConfigurationChangeCallback(builder);

            builder.Services.AddSingleton(new Options { 
                ApplicationContext = configuration["Logging:ContextName"]
            });
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, AsyncLoggerProvider>());
        }

        /// <summary>
        /// 使用Jack.RemoteLog异步远程日志
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configuration"></param>
        public static void UseJackRemoteLogger(this ILoggingBuilder builder, IConfiguration configuration,Options options)
        {
            if (options == null)
                throw new ArgumentException("options is null");

            Global.Configuration = configuration;
            if(string.IsNullOrWhiteSpace(options.ApplicationContext))
            {
                options.ApplicationContext = configuration["Logging:ContextName"];
            }
            if (!string.IsNullOrEmpty(options.UserName))
            {
                var authorizationStr = $"{options.UserName}:{options.Password}";
                Global.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes(authorizationStr)));
            }

            ConfigurationChangeCallback(builder);


            builder.Services.AddSingleton(options);
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, AsyncLoggerProvider>());
        }

    }
}
