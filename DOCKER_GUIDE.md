# ?? Docker Deployment Guide

## Overview
The MeDirect Trading Service is fully dockerized and can be deployed using Docker Compose. This guide explains how to run the entire application stack in containers.

## ??? Architecture in Docker

```
????????????????????????    ????????????????????????    ????????????????????????
?   trading-api        ?    ?   postgres           ?    ?   rabbitmq           ?
?   (Port: 8000)       ??????   (Port: 5432)       ?    ?   (Port: 5672/15672) ?
?   .NET 8 Web API     ?    ?   PostgreSQL 15      ?    ?   RabbitMQ 3         ?
????????????????????????    ????????????????????????    ????????????????????????
                                        ?                           ?
                                        ?                           ?
????????????????????????    ????????????????????????    ????????????????????????
?   trading-consumer   ?    ?   adminer            ?    ?   Docker Network     ?
?   (Console App)      ?    ?   (Port: 8080)       ?    ?   trading_network    ?
?   .NET 8 Console     ?    ?   DB Admin Tool      ?    ?                      ?
????????????????????????    ????????????????????????    ????????????????????????
```

## ?? Quick Start with Docker

### Option 1: Use the Deployment Script (Recommended)
```bash
# Windows
deploy-docker.bat

# Select option 1 for full deployment
```

### Option 2: Manual Docker Commands
```bash
# Build and start all services
docker-compose up --build -d

# Check status
docker-compose ps

# View logs
docker-compose logs -f
```

## ?? Docker Services

| Service | Container Name | Port | Description |
|---------|----------------|------|-------------|
| trading-api | trading_api | 8000 | Main API service |
| trading-consumer | trading_consumer | - | Message consumer |
| postgres | trading_postgres | 5432 | PostgreSQL database |
| rabbitmq | trading_rabbitmq | 5672, 15672 | Message queue |
| adminer | trading_adminer | 8080 | Database admin |

## ?? Service URLs (Docker)

When running in Docker, use these URLs:

- **?? Trading API**: http://localhost:8000
- **?? Swagger UI**: http://localhost:8000
- **?? Health Check**: http://localhost:8000/health
- **??? Database Admin**: http://localhost:8080
- **?? RabbitMQ Management**: http://localhost:15672

## ?? Environment Configuration

The Docker services use these environment variables:

### Trading API (trading-api)
```yaml
environment:
  - ASPNETCORE_ENVIRONMENT=Production
  - ASPNETCORE_URLS=http://+:80
  - UseInMemoryDatabase=false
  - ConnectionStrings__DefaultConnection=Host=postgres;Port=5432;Database=TradingServiceDb;Username=postgres;Password=postgres;
  - RabbitMQ__HostName=rabbitmq
```

### Trading Consumer (trading-consumer)
```yaml
environment:
  - RabbitMQ__HostName=rabbitmq
  - RabbitMQ__Port=5672
  - RabbitMQ__UserName=guest
  - RabbitMQ__Password=guest
```

## ?? Management Commands

### Start Services
```bash
# Start all services
docker-compose up -d

# Start specific services
docker-compose up postgres rabbitmq -d
```

### Stop Services
```bash
# Stop all services
docker-compose down

# Stop and remove volumes (deletes data)
docker-compose down -v
```

### View Logs
```bash
# All services
docker-compose logs -f

# Specific service
docker-compose logs -f trading-api
docker-compose logs -f trading-consumer
docker-compose logs -f postgres
docker-compose logs -f rabbitmq
```

### Check Status
```bash
# Service status
docker-compose ps

# Container resource usage
docker stats
```

### Database Access
```bash
# Connect to PostgreSQL
docker exec -it trading_postgres psql -U postgres -d TradingServiceDb

# SQL commands inside PostgreSQL
\dt                    # List tables
SELECT * FROM trades;  # View trades
\q                     # Quit
```

## ??? Building Custom Images

### Build API Image
```bash
# Build from TradingService directory
docker build -t medirect-trading-api -f TradingService/Dockerfile .
```

### Build Consumer Image
```bash
# Build from root directory
docker build -t medirect-trading-consumer -f TradingConsumer/Dockerfile .
```

### Build All Images
```bash
# Build all services defined in docker-compose.yml
docker-compose build

# Build without cache
docker-compose build --no-cache
```

## ?? Monitoring and Troubleshooting

### Health Checks
Docker Compose includes health checks for PostgreSQL and RabbitMQ:

```bash
# Check health status
docker-compose ps

# View health check logs
docker inspect trading_postgres | grep -A 5 "Health"
```

### Common Issues

#### 1. Port Conflicts
If ports are already in use:
```bash
# Check what's using the port
netstat -ano | findstr :8000

# Kill the process (Windows)
taskkill /PID <process_id> /F
```

#### 2. Database Connection Issues
```bash
# Check if PostgreSQL is ready
docker exec trading_postgres pg_isready -U postgres

# View PostgreSQL logs
docker-compose logs postgres
```

#### 3. RabbitMQ Connection Issues
```bash
# Check RabbitMQ status
docker exec trading_rabbitmq rabbitmq-diagnostics ping

# View RabbitMQ logs
docker-compose logs rabbitmq
```

#### 4. Build Issues
```bash
# Clean build cache
docker builder prune

# Remove all containers and rebuild
docker-compose down
docker-compose build --no-cache
docker-compose up -d
```

## ?? Performance Optimization

### Resource Limits
Add resource limits to docker-compose.yml:

```yaml
services:
  trading-api:
    deploy:
      resources:
        limits:
          cpus: '1.0'
          memory: 512M
        reservations:
          cpus: '0.5'
          memory: 256M
```

### Volume Optimization
```bash
# Use named volumes for better performance
volumes:
  postgres_data:
    driver: local
    driver_opts:
      type: none
      o: bind
      device: /path/to/data
```

## ?? Security Considerations

### Production Deployment
For production, update these settings:

1. **Change default passwords**:
```yaml
environment:
  POSTGRES_PASSWORD: your_secure_password
  RABBITMQ_DEFAULT_PASS: your_secure_password
```

2. **Use secrets**:
```yaml
secrets:
  postgres_password:
    file: ./secrets/postgres_password.txt
```

3. **Enable TLS**:
```yaml
environment:
  ASPNETCORE_URLS: https://+:443;http://+:80
  ASPNETCORE_Kestrel__Certificates__Default__Path: /app/certificates/cert.pfx
```

## ?? Testing the Dockerized Application

### API Testing
```bash
# Health check
curl http://localhost:8000/health

# Create a trade
curl -X POST "http://localhost:8000/api/trades" \
  -H "Content-Type: application/json" \
  -d '{
    "symbol": "AAPL",
    "quantity": 100,
    "price": 150.50,
    "tradeType": 1,
    "userId": "dockertest"
  }'

# Get trades
curl "http://localhost:8000/api/trades?userId=dockertest"
```

### Message Queue Testing
1. Create a trade via API
2. Check RabbitMQ Management UI: http://localhost:15672
3. Verify consumer logs: `docker-compose logs trading-consumer`

## ?? Deployment to Production

### Docker Swarm
```bash
# Initialize swarm
docker swarm init

# Deploy stack
docker stack deploy -c docker-compose.yml trading-service
```

### Kubernetes
Convert docker-compose.yml to Kubernetes manifests:
```bash
# Install kompose
kompose convert

# Apply to Kubernetes
kubectl apply -f .
```

## ?? Updates and Maintenance

### Update Application
```bash
# Pull latest code
git pull

# Rebuild and redeploy
docker-compose build --no-cache
docker-compose up -d
```

### Backup Database
```bash
# Create backup
docker exec trading_postgres pg_dump -U postgres TradingServiceDb > backup.sql

# Restore backup
docker exec -i trading_postgres psql -U postgres TradingServiceDb < backup.sql
```

### Clean Up
```bash
# Remove unused images
docker image prune

# Remove unused volumes
docker volume prune

# Complete cleanup
docker system prune -a
```