
# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 9007

# Install curl for healthcheck
USER root
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*
USER $APP_UID


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["ae-poc-identity-mcp-srvsse/ae-poc-identity-mcp-srvsse.csproj", "ae-poc-identity-mcp-srvsse/"]
COPY ["ae-poc-identity-mcp-lib/ae-poc-identity-mcp-lib.csproj", "ae-poc-identity-mcp-lib/"]
RUN dotnet restore "./ae-poc-identity-mcp-srvsse/ae-poc-identity-mcp-srvsse.csproj"
COPY . .
WORKDIR "/src/ae-poc-identity-mcp-srvsse"
RUN dotnet build "./ae-poc-identity-mcp-srvsse.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ae-poc-identity-mcp-srvsse.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
HEALTHCHECK --interval=30s --timeout=30s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:9007/health/live || exit 1
ENTRYPOINT ["dotnet", "ae-poc-identity-mcp-srvsse.dll"]