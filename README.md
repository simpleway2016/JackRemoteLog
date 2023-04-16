# JackRemoteLog
JackRemoteLog 是一个基于.Net，支持全文检索的远程日志组件

Jack.RemoteLog.WebApi 是服务器端。

## 使用
部署好服务器端，工程中引用nuget包：Jack.RemoteLog
在配置文件中，设置服务器url
``` json
  "Logging": {
    "ServerUrl": "http://127.0.0.1:9000",
    "ContextName": "YourContextName",
    "LogLevel": {
      "Default": "Debug"
    },
    "Console": {
      "LogLevel": {
        "Default": "Information"
      }
    }
  }
```
注册组件：
``` cs
            services.AddLogging(builder =>
            {
                builder.AddConfiguration(configuration.GetSection("Logging"));
                builder.AddConsole();
                builder.UseJackRemoteLogger(configuration);
            });
```
