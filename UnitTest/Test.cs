using Jack.RemoteLog;
using Jack.RemoteLog.WebApi;
using Jack.RemoteLog.WebApi.Applications;
using Jack.RemoteLog.WebApi.AutoMissions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;
using System;

namespace UnitTest
{
    [TestClass]
    public class Test
    {
        ServiceCollection _services;
        ServiceProvider _serviceProvider;
        public Test()
        {
            ConfigurationBuilder builder = new ConfigurationBuilder();
            builder.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            _services = new ServiceCollection();
            _services.AddSingleton<LogChannelRoute>();
            _services.AddSingleton<LogService>();
            _services.AddSingleton<IConfiguration>(Global.Configuration = builder.Build());

            _services.AddLogging(builder =>
            {
                builder.UseJackRemoteLogger(Global.Configuration, "MyApplicationContext");
            });

            Global.ServiceProvider = _serviceProvider = _services.BuildServiceProvider();
           
        }

        [TestMethod]
        public void TestLogger()
        {
            Global.ServiceProvider.GetService<ILogger<Test>>().LogDebug("hello");
            Thread.Sleep(1000);
        }


        [TestMethod]
        public void WriteMuilteLog()
        {
            var logService = _serviceProvider.GetService<LogService>();
            for (int i = 0; i < 1000000; i++)
            {
                Thread.Sleep(10);
                logService.WriteLog(new Jack.RemoteLog.WebApi.Dtos.WriteLogModel
                {
                    ApplicationContext = "UnitTest",
                    SourceContext = "test",
                    Content = "²âÊÔ\r\nÈÕÖ¾",
                    Level = Microsoft.Extensions.Logging.LogLevel.Debug,
                    Timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds()
                });
            }
        }
    }
}