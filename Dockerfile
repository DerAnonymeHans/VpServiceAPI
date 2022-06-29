FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /VpServiceAPI

COPY ./VpService.csproj
COPY . ./

CMD ASPNETCORE_URLS=http://*:$PORT dotnet VpService.dll
