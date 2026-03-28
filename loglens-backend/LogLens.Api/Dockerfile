FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["LogLens.Api/LogLens.Api.csproj", "LogLens.Api/"]
COPY ["LogLens.Core/LogLens.Core.csproj", "LogLens.Core/"]
RUN dotnet restore "LogLens.Api/LogLens.Api.csproj"
COPY . .
RUN dotnet build "LogLens.Api/LogLens.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "LogLens.Api/LogLens.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LogLens.Api.dll"]
