@chcp 65001
set version=1.0.7
dotnet publish ..\Jack.RemoteLog.WebApi\Jack.RemoteLog.WebApi.csproj -c release -o Publish\Linux\RemoteLogServer --self-contained false --runtime linux-x64

docker build -t jackframework/jackremotelogwebapi:%version% -f dockerfile_remotelog .
@echo 现在让网络可以访问docker
pause
docker push jackframework/jackremotelogwebapi:%version%
docker tag jackframework/jackremotelogwebapi:%version% jackframework/jackremotelogwebapi:latest
docker push jackframework/jackremotelogwebapi:latest
pause