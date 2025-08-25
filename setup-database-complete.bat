@echo off
cls
echo ?? Complete Setup - Trading Service with Database Access
echo =========================================================
echo.

echo This script will:
echo ? Start PostgreSQL database
echo ? Start Adminer for database management  
echo ? Verify all services are working
echo ? Guide you through accessing the database
echo.

pause

echo ?? Starting setup process...
echo.

echo Step 1/5: ?? Stopping any existing containers...
docker-compose down >nul 2>&1
echo ? Containers stopped

echo.
echo Step 2/5: ?? Starting PostgreSQL and Adminer...
docker-compose up postgres adminer -d
echo ? Services started

echo.
echo Step 3/5: ? Waiting for services to initialize (30 seconds)...
echo This gives time for PostgreSQL and Adminer to fully start up...
for /l %%i in (30,-1,1) do (
    set /p="? %%i seconds remaining..." <nul
    timeout /t 1 >nul
    echo.
)

echo.
echo Step 4/5: ?? Verifying services are running...
echo.
echo Container Status:
docker ps --filter "name=trading_postgres" --filter "name=trading_adminer" --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"

echo.
echo PostgreSQL Health Check:
docker exec trading_postgres pg_isready -U postgres 2>nul
if errorlevel 1 (
    echo ? PostgreSQL is not ready yet
) else (
    echo ? PostgreSQL is ready
)

echo.
echo Adminer Access Test:
powershell -Command "try { Invoke-WebRequest -Uri 'http://localhost:8080' -UseBasicParsing -TimeoutSec 5 | Out-Null; Write-Host '? Adminer is accessible' -ForegroundColor Green } catch { Write-Host '?? Adminer is starting up... try in a moment' -ForegroundColor Yellow }"

echo.
echo Step 5/5: ??? Database Setup Instructions...
echo.
echo ???????????????????????????????????????????????????????????????????????
echo ?? DATABASE ACCESS - Adminer Web UI
echo ???????????????????????????????????????????????????????????????????????
echo.
echo ?? URL: http://localhost:8080
echo.
echo ?? Login Credentials:
echo   System: PostgreSQL
echo   Server: postgres
echo   Username: postgres  
echo   Password: postgres
echo   Database: TradingServiceDb
echo.
echo ?? Once logged in, you can:
echo   • View table "trades" with: SELECT * FROM trades;
echo   • See total records: SELECT COUNT(*) FROM trades;
echo   • Filter by user: SELECT * FROM trades WHERE user_id = 'testuser';
echo ???????????????????????????????????????????????????????????????????????
echo.

set /p open_adminer="?? Open Adminer in browser now? (Y/n): "
if /i not "%open_adminer%"=="n" (
    echo Opening Adminer...
    start http://localhost:8080
    echo.
    echo ?? If the page doesn't load immediately, wait 30 seconds and refresh
)

echo.
echo ?? Setup Complete! 
echo.
echo ?? Next Steps:
echo 1. ?? Access database via: http://localhost:8080
echo 2. ?? Start your Trading API: cd TradingService && dotnet run
echo 3. ?? Test API via: http://localhost:5100
echo 4. ?? Create trades and see them appear in the database!
echo.
echo ?? Useful Commands:
echo   • View containers: docker ps
echo   • Restart Adminer: docker-compose restart adminer
echo   • Stop all: docker-compose down
echo   • View logs: docker logs trading_adminer
echo.

pause