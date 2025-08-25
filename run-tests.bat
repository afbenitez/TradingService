@echo off
cls
echo Running Unit Tests - MeDirect Trading Service
echo ===============================================
echo.

echo Checking test environment...
dotnet --version >nul 2>&1
if errorlevel 1 (
    echo ERROR: .NET SDK not found
    pause
    exit /b 1
)
echo SUCCESS: .NET SDK found

echo.
echo Test execution options:
echo.
echo 1. Run all tests
echo 2. Run unit tests only (Services + Validators)
echo 3. Run integration tests only
echo 4. Run tests with coverage report
echo 5. Run specific test class
echo 6. List all available tests
echo 7. Exit
echo.

set /p choice="Select option (1-7): "

if "%choice%"=="1" goto run_all
if "%choice%"=="2" goto run_unit
if "%choice%"=="3" goto run_integration
if "%choice%"=="4" goto run_coverage
if "%choice%"=="5" goto run_specific
if "%choice%"=="6" goto list_tests
if "%choice%"=="7" goto exit

echo Invalid choice. Please select 1-7.
pause
goto start

:run_all
echo.
echo Running all tests...
echo.
cd TradingService.Tests
dotnet test --verbosity normal --logger "console;verbosity=detailed"
goto show_results

:run_unit
echo.
echo Running unit tests...
echo.
cd TradingService.Tests
dotnet test --filter "FullyQualifiedName~TradingService.Tests.Services|FullyQualifiedName~TradingService.Tests.Validators" --verbosity normal
goto show_results

:run_integration
echo.
echo Running integration tests...
echo.
cd TradingService.Tests
dotnet test --filter "FullyQualifiedName~TradingService.Tests.Integration" --verbosity normal
goto show_results

:run_coverage
echo.
echo Running tests with coverage...
echo Installing coverlet if needed...
dotnet tool install --global coverlet.console 2>nul
cd TradingService.Tests
dotnet test --collect:"XPlat Code Coverage" --logger "trx;LogFileName=TestResults.trx"
echo.
echo Coverage report generated in TestResults folder
goto show_results

:run_specific
echo.
echo Available test classes:
echo 1. TradeServiceTests
echo 2. CreateTradeDtoValidatorTests  
echo 3. TradingApiIntegrationTests
echo.
set /p test_class="Enter test class number (1-3): "

if "%test_class%"=="1" set filter_name=TradeServiceTests
if "%test_class%"=="2" set filter_name=CreateTradeDtoValidatorTests
if "%test_class%"=="3" set filter_name=TradingApiIntegrationTests

if not defined filter_name (
    echo Invalid selection
    pause
    goto start
)

echo.
echo Running %filter_name%...
cd TradingService.Tests
dotnet test --filter "FullyQualifiedName~%filter_name%" --verbosity normal
goto show_results

:list_tests
echo.
echo Discovering all tests...
echo.
cd TradingService.Tests
dotnet test --list-tests
pause
goto start

:show_results
echo.
echo ===============================================
echo Test Execution Completed!
echo ===============================================
echo.
echo Test Summary:
echo - Check results above for pass/fail status
echo - All tests should pass for a successful build
echo - Any failures indicate areas that need attention
echo.
echo Useful commands:
echo   Run specific test: dotnet test --filter "TestMethodName"
echo   Run with coverage: dotnet test --collect:"XPlat Code Coverage"
echo   Continuous testing: dotnet watch test
echo.

set /p run_again="Run tests again? (y/N): "
if /i "%run_again%"=="y" goto start

cd ..
pause
exit /b 0

:exit
echo.
echo Goodbye!
exit /b 0

:start
cls
goto start