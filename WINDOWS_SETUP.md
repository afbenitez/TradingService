# Windows Setup Instructions

## Quick Start for Windows Users

Since you're using Windows, you have several options to run the Trading Service:

### Option 1: Simple Batch Files (Recommended)
```cmd
# Start everything automatically
setup.bat

# Or run individual commands:
start-service.bat    # Start the service
test-api.bat        # Test the API
setup-database.bat  # Setup database only
```

### Option 2: PowerShell Script (Advanced)
```powershell
# Allow script execution (run as Administrator)
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser

# Run the complete setup
.\setup.ps1 -Action setup

# Or run individual tasks:
.\setup.ps1 -Action infrastructure
.\setup.ps1 -Action database
.\setup.ps1 -Action service
.\setup.ps1 -Action test
```

### Option 3: Manual Commands
```cmd
# 1. Start infrastructure
docker-compose up postgres rabbitmq -d

# 2. Setup database
cd TradingService
dotnet ef migrations add InitialCreate --output-dir Data/Migrations
dotnet ef database update

# 3. Start the service
dotnet run
```

## Why chmod doesn't work on Windows

The `chmod` command is a Unix/Linux command for setting file permissions. On Windows, you don't need it because:

1. **Batch files (.bat)** are automatically executable
2. **PowerShell scripts (.ps1)** use execution policies instead
3. **File permissions** are handled differently in Windows

## If you see PowerShell execution policy errors:

```powershell
# Run this as Administrator to allow local scripts:
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser

# Or bypass for a single execution:
PowerShell -ExecutionPolicy Bypass -File setup.ps1
```

## Troubleshooting

### Docker not found
- Install Docker Desktop from https://www.docker.com/products/docker-desktop
- Make sure Docker Desktop is running

### .NET not found
- Install .NET 8 SDK from https://dotnet.microsoft.com/download/dotnet/8.0

### Port conflicts
- If port 7179 or 5100 is busy, change ports in `launchSettings.json`
- Or kill the process using the port: `netstat -ano | findstr :7179`

### Certificate issues
```cmd
# Trust the development certificate
dotnet dev-certs https --trust
```

## Service URLs

Once running, access:
- **Swagger UI**: https://localhost:7179/
- **API**: https://localhost:7179/api/trades
- **Health Check**: https://localhost:7179/health
- **RabbitMQ Management**: http://localhost:15672 (guest/guest)

## Testing the API

```cmd
# Using the test script
test-api.bat

# Or manually with curl (if installed) or PowerShell:
curl -k -X POST "https://localhost:7179/api/trades" -H "Content-Type: application/json" -d "{\"symbol\":\"AAPL\",\"quantity\":100,\"price\":150.50,\"tradeType\":1,\"userId\":\"testuser\"}"
```