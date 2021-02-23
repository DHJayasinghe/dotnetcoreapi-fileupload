FROM mcr.microsoft.com/dotnet/aspnet:5.0.2-alpine3.12-amd64 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:5.0.102-1-alpine3.12-amd64 AS build
WORKDIR /src
COPY ["DotnetCoreApi.FileUpload.csproj", ""]
RUN dotnet restore "./DotnetCoreApi.FileUpload.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "DotnetCoreApi.FileUpload.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DotnetCoreApi.FileUpload.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DotnetCoreApi.FileUpload.dll"]