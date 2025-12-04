# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
USER $APP_UID
WORKDIR /app


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["ae-poc-identity-mcp-srvsse/ae-poc-identity-mcp-srvsse.csproj", "ae-poc-identity-mcp-srvsse/"]
RUN dotnet restore "./ae-poc-identity-mcp-srvsse/ae-poc-identity-mcp-srvsse.csproj"
COPY . .
WORKDIR "/src/ae-poc-identity-mcp-srvsse"
RUN dotnet build "./ae-poc-identity-mcp-srvsse.csproj" -c $BUILD_CONFIGURATION -o /app/build

# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
USER $APP_UID
WORKDIR /app


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["ae-poc-identity-mcp-srvsse/ae-poc-identity-mcp-srvsse.csproj", "ae-poc-identity-mcp-srvsse/"]
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
EXPOSE 8080
HEALTHCHECK --interval=30s --timeout=30s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:8080/health || exit 1
ENTRYPOINT ["dotnet", "ae-poc-identity-mcp-srvsse.dll"]