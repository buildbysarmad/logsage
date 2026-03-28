FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["LogSage.Api/LogSage.Api.csproj", "LogSage.Api/"]
COPY ["LogSage.Core/LogSage.Core.csproj", "LogSage.Core/"]
RUN dotnet restore "LogSage.Api/LogSage.Api.csproj"
COPY . .
RUN dotnet build "LogSage.Api/LogSage.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "LogSage.Api/LogSage.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LogSage.Api.dll"]
