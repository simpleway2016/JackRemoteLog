using Jack.RemoteLog;
using Jack.RemoteLog.WebApi;
using Jack.RemoteLog.WebApi.Applications;
using Jack.RemoteLog.WebApi.AutoMissions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Diagnostics;

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
            _services.AddScoped<TestObject>();

            Global.ServiceProvider = _serviceProvider = _services.BuildServiceProvider();
           
        }

        [TestMethod]
        public void TestLogger()
        {
            Global.ServiceProvider.GetService<ILogger<Test>>().LogError(new Exception("abc") , "错误异常");
            Global.ServiceProvider.GetService<ILogger<Test>>().LogInformation("normal");
            Thread.Sleep(1000);
        }



        [TestMethod]
        public void WriteMuilteLog()
        {
            var logService = _serviceProvider.GetService<LogService>();
            for (int i = 0; i < 1000000; i++)
            {
               
                logService.WriteLog(new Jack.RemoteLog.WebApi.Dtos.WriteLogModel
                {
                    ApplicationContext = "UnitTest",
                    SourceContext = "test",
                    Content = "Exchange:Exchange.BlockScan 收到BlockScan信息：{\"BlockNumber\":43325451,\"Txid\":\"04b394121551767dd7237d8fcaf84eab31f102f3bd03122b94bf784581217ec5\",\"Amount\":99999.0,\"Time\":\"1970-01-01T00:00:00+00:00\",\"Confirmations\":3,\"Valid\":true,\"PropertyId\":\"TR7NHqjeKQxGTCi8q8ZY4pL8otSzgjLj6t\",\"CoinType\":202,\"TronTransactionType\":\"TriggerSmartContract\",\"ContractRet\":\"SUCCESS\",\"Fee\":0.0,\"SenderAddress\":\"TBA6CypYJizwA9XdC7Ubgc5F1bxrQ7SqPt\",\"ReceivedAddress\":\"TVKCgFfuuzu11idqBjqMoSUDvmVQYnJWwY\",\"Coin\":\"USDT\"}",
                    Level = Microsoft.Extensions.Logging.LogLevel.Debug,
                    Timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds()
                });

                if(i % 1000 == 0)
                    Thread.Sleep(1);
            }
        }
    }

    class TestObject:IDisposable
    {
        public TestObject()
        {
            Debug.WriteLine("运行构造函数");
        }

        public void Dispose()
        {
            Debug.WriteLine("释放了");
        }
    }
}