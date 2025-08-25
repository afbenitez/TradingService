# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY TradingService/TradingService.csproj TradingService/
RUN dotnet restore TradingService/TradingService.csproj

# Copy source code and build
COPY TradingService/ TradingService/
WORKDIR /src/TradingService
RUN dotnet build TradingService.csproj -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish TradingService.csproj -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create logs directory
RUN mkdir -p /app/logs

# Expose port
EXPOSE 80
EXPOSE 443

ENTRYPOINT ["dotnet", "TradingService.dll"]