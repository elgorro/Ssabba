# Build from the repository root: docker build -f src/Ssabba.Web/Dockerfile .
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore first so package layers cache independently of source changes.
COPY Directory.Build.props Directory.Packages.props nuget.config Ssabba.slnx ./
COPY src/Ssabba.Domain/Ssabba.Domain.csproj src/Ssabba.Domain/
COPY src/Ssabba.Shared/Ssabba.Shared.csproj src/Ssabba.Shared/
COPY src/Ssabba.Infrastructure/Ssabba.Infrastructure.csproj src/Ssabba.Infrastructure/
COPY src/Ssabba.Web/Ssabba.Web.csproj src/Ssabba.Web/
COPY src/Ssabba.Web.Client/Ssabba.Web.Client.csproj src/Ssabba.Web.Client/
COPY tests/Ssabba.Domain.Tests/Ssabba.Domain.Tests.csproj tests/Ssabba.Domain.Tests/
RUN dotnet restore src/Ssabba.Web/Ssabba.Web.csproj

COPY . .
RUN dotnet publish src/Ssabba.Web/Ssabba.Web.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
# curl is only here so the container healthcheck can hit /healthz.
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8080 \
    DOTNET_RUNNING_IN_CONTAINER=true
EXPOSE 8080
# Data-protection keys live on a volume; Docker copies this ownership onto an empty volume.
RUN mkdir -p /keys && chown $APP_UID /keys
USER $APP_UID
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Ssabba.Web.dll"]
