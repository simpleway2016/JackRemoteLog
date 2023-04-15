#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Jack.RemoteLog.WebApi/Jack.RemoteLog.WebApi.csproj", "Jack.RemoteLog.WebApi/"]
RUN dotnet restore "Jack.RemoteLog.WebApi/Jack.RemoteLog.WebApi.csproj"
COPY . .
WORKDIR "/src/Jack.RemoteLog.WebApi"
RUN dotnet build "Jack.RemoteLog.WebApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Jack.RemoteLog.WebApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Jack.RemoteLog.WebApi.dll","-s:share"]