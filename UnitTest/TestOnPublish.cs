using Jack.RemoteLog.WebApi;
using Jack.RemoteLog.WebApi.Applications;
using Jack.RemoteLog.WebApi.AutoMissions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using System;

namespace UnitTest
{
    [TestClass]
    public class TestOnPublish
    {
        ServiceCollection _services;
        ServiceProvider _serviceProvider;
        public TestOnPublish()
        {
            ConfigurationBuilder builder = new ConfigurationBuilder();
            builder.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            _services = new ServiceCollection();
            _services.AddSingleton<LogChannelRoute>();
            _services.AddSingleton<LogService>();
            _services.AddSingleton<IConfiguration>(Global.Configuration = builder.Build());
            Global.ServiceProvider = _serviceProvider = _services.BuildServiceProvider();
        }

        [TestMethod]
        public void WriteLog()
        {
            var logService = _serviceProvider.GetService<LogService>();
            for(int i = 0; i < 1000; i++)
            {
                logService.WriteLog(new Jack.RemoteLog.WebApi.Dtos.WriteLogModel
                {
                    ApplicationContext = "UnitTest",
                    SourceContext = "test",
                    Content = i + "≤‚ ‘\r\n»’÷æ",
                    Level = Microsoft.Extensions.Logging.LogLevel.Debug,
                    Timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds()
                });
            }
            Thread.Sleep(3000);
        }


        [TestMethod]
        public void ReadLog()
        {
            var logService = _serviceProvider.GetService<LogService>();
            var logs = logService.ReadLogs("UnitTest", "test", Microsoft.Extensions.Logging.LogLevel.Debug, DateTimeOffset.FromFileTime(DateTime.Now.AddDays(-3).ToFileTime()).ToUnixTimeMilliseconds(), null, "≤‚ ‘");
        }

        [TestMethod]
        public void GetSourceContextes()
        {
            var logService = _serviceProvider.GetService<LogService>();
            var ret = logService.GetSourceContextes("UnitTest");
        }

        [TestMethod]
        public void DeleteFile()
        {
            var logService = _serviceProvider.GetService<LogService>();
            var logs = logService.ReadLogs("TradeSystem.AssetServiceHost-test",null,null, DateTimeOffset.FromFileTime(DateTime.Now.AddDays(-3).ToFileTime()).ToUnixTimeMilliseconds(), null, null);

            ((IJob)new AutoDeleteLogs()).Execute(null).Wait();
            Thread.Sleep(5000);
        }

    }
}