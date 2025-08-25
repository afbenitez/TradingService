@echo off
cls
echo Start Database Services - Quick Fix
echo ===================================
echo.

echo This script will start PostgreSQL + Adminer for database access
echo.

echo Step 1: Stopping any existing containers...
docker-compose down >nul 2>&1

echo Step 2: Starting PostgreSQL and Adminer...
docker-compose up postgres adminer -d

echo Step 3: Waiting for services to be ready...
echo Please wait 20 seconds...
timeout /t 20 >nul

echo Step 4: Checking if services are running...
docker ps --filter "name=trading_postgres" --filter "name=trading_adminer" --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"

echo.
echo Step 5: Testing Adminer access...
powershell -Command "try { Invoke-WebRequest -Uri 'http://localhost:8080' -UseBasicParsing -TimeoutSec 5 | Out-Null; Write-Host 'Success! Adminer is accessible' -ForegroundColor Green } catch { Write-Host 'Adminer is not yet accessible. Wait a bit more and try manually.' -ForegroundColor Red }"

echo.
echo Access Information:
echo ===============================================================
echo Database Admin: http://localhost:8080
echo   System: PostgreSQL
echo   Server: postgres  
echo   Username: postgres
echo   Password: postgres
echo   Database: TradingServiceDb
echo ===============================================================
echo.

set /p open_now="Open Adminer in browser now? (y/N): "
if /i "%open_now%"=="y" (
    start http://localhost:8080
)

echo.
echo If it doesn't work:
echo - Wait a bit more - Adminer can take up to 1 minute to start
echo - Check container status: docker ps
echo - Check logs: docker logs trading_adminer
echo - Try manual restart: docker-compose restart adminer
echo.

pause