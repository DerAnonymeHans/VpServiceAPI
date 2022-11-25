FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build-env
WORKDIR /VpServiceAPI

# Copy everything
COPY . ./
# Restore as distinct layers
RUN dotnet restore
# Build and publish a release
RUN dotnet publish -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:5.0
WORKDIR /VpServiceAPI
COPY --from=build-env /VpServiceAPI/out .
ENTRYPOINT ["dotnet", "VpServiceAPI.dll"]



#COPY . ./

#CMD ASPNETCORE_URLS=http://*:$PORT dotnet VpService.dll
