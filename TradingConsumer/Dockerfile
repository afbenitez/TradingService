# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY TradingConsumer/TradingConsumer.csproj TradingConsumer/
RUN dotnet restore TradingConsumer/TradingConsumer.csproj

# Copy source code and build
COPY TradingConsumer/ TradingConsumer/
WORKDIR /src/TradingConsumer
RUN dotnet build TradingConsumer.csproj -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish TradingConsumer.csproj -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create logs directory
RUN mkdir -p /app/logs

ENTRYPOINT ["dotnet", "TradingConsumer.dll"]