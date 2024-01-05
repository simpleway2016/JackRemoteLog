set version=2.0.1
dotnet publish ..\Jack.RemoteLog.WebApi\Jack.RemoteLog.WebApi.csproj -c release -o Publish\Linux\RemoteLogServer --self-contained true --runtime linux-x64
"C:\Program Files\WinRAR\winrar.exe" a -ep1 %~dp0RemoteLogServer.%version%.linux.zip %~dp0Publish\Linux\RemoteLogServer

dotnet publish ..\Jack.RemoteLog.WebApi\Jack.RemoteLog.WebApi.csproj -c release -o Publish\Windows\RemoteLogServer --self-contained true --runtime win-x64
"C:\Program Files\WinRAR\winrar.exe" a -ep1 %~dp0RemoteLogServer.%version%.win.zip %~dp0Publish\Windows\RemoteLogServer
pause