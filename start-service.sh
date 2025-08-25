#!/bin/bash

echo "?? Starting Trading Service..."
echo ""

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

echo -e "${BLUE}==== Trading Service Startup Script ====${NC}"
echo ""

# Check if Docker is running
if ! docker version > /dev/null 2>&1; then
    echo -e "${RED}? Docker is not running. Please start Docker first.${NC}"
    exit 1
fi

echo -e "${GREEN}? Docker is running${NC}"

# Start infrastructure services
echo -e "${YELLOW}?? Starting PostgreSQL and RabbitMQ...${NC}"
docker-compose up postgres rabbitmq -d

# Wait for services to be ready
echo -e "${YELLOW}? Waiting for services to be ready...${NC}"
sleep 10

# Check PostgreSQL
echo -e "${YELLOW}?? Checking PostgreSQL connection...${NC}"
for i in {1..30}; do
    if docker exec trading_postgres pg_isready -U postgres > /dev/null 2>&1; then
        echo -e "${GREEN}? PostgreSQL is ready${NC}"
        break
    fi
    if [ $i -eq 30 ]; then
        echo -e "${RED}? PostgreSQL failed to start${NC}"
        exit 1
    fi
    sleep 1
done

# Check RabbitMQ
echo -e "${YELLOW}?? Checking RabbitMQ connection...${NC}"
for i in {1..30}; do
    if docker exec trading_rabbitmq rabbitmq-diagnostics ping > /dev/null 2>&1; then
        echo -e "${GREEN}? RabbitMQ is ready${NC}"
        break
    fi
    if [ $i -eq 30 ]; then
        echo -e "${RED}? RabbitMQ failed to start${NC}"
        exit 1
    fi
    sleep 1
done

# Setup database if needed
if [ ! -f "TradingService/Data/Migrations/InitialCreate.cs" ]; then
    echo -e "${YELLOW}???  Setting up database...${NC}"
    cd TradingService
    dotnet ef migrations add InitialCreate --output-dir Data/Migrations
    dotnet ef database update
    cd ..
    echo -e "${GREEN}? Database setup completed${NC}"
else
    echo -e "${GREEN}? Database already configured${NC}"
fi

# Build the project
echo -e "${YELLOW}?? Building Trading Service...${NC}"
cd TradingService
dotnet build
if [ $? -eq 0 ]; then
    echo -e "${GREEN}? Build successful${NC}"
else
    echo -e "${RED}? Build failed${NC}"
    exit 1
fi

echo ""
echo -e "${GREEN}?? Setup completed successfully!${NC}"
echo ""
echo -e "${BLUE}?? Service Information:${NC}"
echo -e "?? API URL: ${GREEN}https://localhost:7179${NC} or ${GREEN}http://localhost:5100${NC}"
echo -e "?? Swagger UI: ${GREEN}https://localhost:7179${NC}"
echo -e "?? Health Check: ${GREEN}https://localhost:7179/health${NC}"
echo -e "?? RabbitMQ Management: ${GREEN}http://localhost:15672${NC} (guest/guest)"
echo -e "???  Database Admin: ${GREEN}http://localhost:8080${NC}"
echo ""
echo -e "${YELLOW}?? Starting Trading Service...${NC}"
echo -e "${BLUE}Press Ctrl+C to stop the service${NC}"
echo ""

# Start the service
dotnet run