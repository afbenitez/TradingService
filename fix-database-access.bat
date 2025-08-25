@echo off
cls
echo Database Access Troubleshooting & Fix
echo ======================================
echo.

echo Step 1: Checking Docker status...
docker --version >nul 2>&1
if errorlevel 1 (
    echo ERROR: Docker is not running or not installed
    echo Please install Docker Desktop: https://www.docker.com/products/docker-desktop
    pause
    exit /b 1
)
echo SUCCESS: Docker is available

echo.
echo Step 2: Checking current containers...
echo Current running containers:
docker ps --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"
echo.

echo Step 3: Checking specific services...
echo Checking PostgreSQL...
docker ps | findstr trading_postgres >nul
if errorlevel 1 (
    echo ERROR: PostgreSQL is not running
) else (
    echo SUCCESS: PostgreSQL container found
)

echo Checking RabbitMQ...
docker ps | findstr trading_rabbitmq >nul
if errorlevel 1 (
    echo ERROR: RabbitMQ is not running
) else (
    echo SUCCESS: RabbitMQ container found
)

echo Checking Adminer...
docker ps | findstr trading_adminer >nul
if errorlevel 1 (
    echo ERROR: Adminer is not running - THIS IS THE PROBLEM
) else (
    echo SUCCESS: Adminer container found
)

echo.
echo Solution Options:
echo.
echo 1. Start ALL services (PostgreSQL + RabbitMQ + Adminer)
echo 2. Start only database services (PostgreSQL + Adminer)
echo 3. Clean restart (stop all and restart fresh)
echo 4. Force rebuild and start
echo 5. Show detailed container status
echo 6. Exit
echo.

set /p choice="Select option (1-6): "

if "%choice%"=="1" goto start_all
if "%choice%"=="2" goto start_db
if "%choice%"=="3" goto clean_restart
if "%choice%"=="4" goto force_rebuild
if "%choice%"=="5" goto detailed_status
if "%choice%"=="6" goto exit

echo Invalid choice. Please select 1-6.
pause
goto start

:start_all
echo.
echo Starting all services...
echo Pulling latest images...
docker-compose pull
echo Starting services...
docker-compose up postgres rabbitmq adminer -d
goto verify

:start_db
echo.
echo Starting database services...
echo Pulling latest images...
docker-compose pull adminer postgres
echo Starting services...
docker-compose up postgres adminer -d
goto verify

:clean_restart
echo.
echo Clean restart process...
echo Step 1: Stopping all containers...
docker-compose down
echo Step 2: Removing any orphaned containers...
docker-compose down --remove-orphans
echo Step 3: Pulling latest images...
docker-compose pull
echo Step 4: Starting fresh...
docker-compose up postgres rabbitmq adminer -d
goto verify

:force_rebuild
echo.
echo Force rebuild and start...
echo Stopping everything...
docker-compose down
echo Removing volumes (this will delete data!)...
set /p confirm="Are you sure you want to delete all data? (y/N): "
if /i "%confirm%"=="y" (
    docker-compose down -v
)
echo Pulling fresh images...
docker-compose pull
echo Building and starting...
docker-compose up postgres rabbitmq adminer -d --force-recreate
goto verify

:detailed_status
echo.
echo Detailed container status...
echo.
echo === ALL CONTAINERS ===
docker ps -a --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}\t{{.Image}}"
echo.
echo === DOCKER COMPOSE STATUS ===
docker-compose ps
echo.
echo === ADMINER SPECIFIC CHECK ===
docker ps -a | findstr adminer
echo.
echo === PORT 8080 CHECK ===
netstat -ano | findstr :8080
echo.
pause
goto start

:verify
echo.
echo Waiting for services to start (30 seconds)...
timeout /t 30 >nul

echo.
echo Verifying services...

REM Check PostgreSQL
echo Checking PostgreSQL...
docker exec trading_postgres pg_isready -U postgres >nul 2>&1
if errorlevel 1 (
    echo ERROR: PostgreSQL is not ready
) else (
    echo SUCCESS: PostgreSQL is ready
)

REM Check RabbitMQ
echo Checking RabbitMQ...
docker exec trading_rabbitmq rabbitmq-diagnostics ping >nul 2>&1
if errorlevel 1 (
    echo ERROR: RabbitMQ is not ready
) else (
    echo SUCCESS: RabbitMQ is ready
)

REM Check Adminer
echo Checking Adminer...
for /l %%i in (1,1,10) do (
    powershell -Command "try { $response = Invoke-WebRequest -Uri 'http://localhost:8080' -UseBasicParsing -TimeoutSec 3 -ErrorAction Stop; Write-Host 'SUCCESS: Adminer is responding' -ForegroundColor Green; exit 0 } catch { Start-Sleep 2; if (%%i -eq 10) { Write-Host 'ERROR: Adminer is not responding after 10 attempts' -ForegroundColor Red } }" >nul 2>&1
    if not errorlevel 1 goto adminer_ready
)

echo WARNING: Adminer might still be starting. Checking container logs...
docker logs trading_adminer --tail 5

:adminer_ready
echo.
echo Current Service Status:
docker ps --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"
echo.
echo Access URLs:
echo ==========================================
echo Database Admin (Adminer): http://localhost:8080
echo   System: PostgreSQL
echo   Server: postgres
echo   Username: postgres
echo   Password: postgres
echo   Database: TradingServiceDb
echo.
echo RabbitMQ Management: http://localhost:15672
echo   Username: guest
echo   Password: guest
echo ==========================================
echo.

echo Final test - Attempting to access Adminer...
powershell -Command "try { $response = Invoke-WebRequest -Uri 'http://localhost:8080' -UseBasicParsing -TimeoutSec 5; Write-Host 'SUCCESS: Adminer is accessible at http://localhost:8080' -ForegroundColor Green; Write-Host 'Opening in browser...' -ForegroundColor Green; Start-Process 'http://localhost:8080' } catch { Write-Host 'FAILED: Adminer is still not accessible' -ForegroundColor Red; Write-Host 'Please check container logs: docker logs trading_adminer' -ForegroundColor Yellow }"

echo.
echo If Adminer still doesn't work:
echo 1. Check logs: docker logs trading_adminer
echo 2. Try restarting: docker-compose restart adminer
echo 3. Check if port 8080 is blocked by firewall
echo 4. Use alternative method: view-database.bat
echo.

pause
goto start

:exit
echo.
echo Goodbye!
exit /b 0

:start
cls
goto start