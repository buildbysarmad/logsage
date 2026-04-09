FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

ARG GITHUB_TOKEN

COPY nuget.config ./
COPY LogSage.Api/LogSage.Api.csproj LogSage.Api/

RUN dotnet nuget remove source github-buildbysarmad 2>/dev/null || true && \
    dotnet nuget add source https://nuget.pkg.github.com/buildbysarmad/index.json \
    --name github-buildbysarmad \
    --username buildbysarmad \
    --password ${GITHUB_TOKEN} \
    --store-password-in-clear-text

RUN dotnet restore LogSage.Api/LogSage.Api.csproj

COPY . .
RUN dotnet publish LogSage.Api/LogSage.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "LogSage.Api.dll"]
