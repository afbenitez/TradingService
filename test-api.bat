@echo off
cls
echo Testing MeDirect Trading Service API
echo ====================================
echo.

set BASE_URL=https://localhost:7179
set HTTP_URL=http://localhost:5100
set API_URL=%BASE_URL%/api/trades

echo Testing if API is running...
echo.

REM Test 1: Health Check
echo 1. Testing Health Check...
echo URL: %BASE_URL%/health
echo.
powershell -Command "try { $response = Invoke-RestMethod -Uri '%BASE_URL%/health' -Method Get -SkipCertificateCheck; Write-Host 'SUCCESS: Health Check Response:' -ForegroundColor Green; $response | ConvertTo-Json } catch { Write-Host 'ERROR: HTTPS Health Check failed, trying HTTP...' -ForegroundColor Yellow; try { $response = Invoke-RestMethod -Uri '%HTTP_URL%/health' -Method Get; Write-Host 'SUCCESS: HTTP Health Check Response:' -ForegroundColor Green; $response | ConvertTo-Json; $global:BASE_URL='%HTTP_URL%'; $global:API_URL='%HTTP_URL%/api/trades' } catch { Write-Host 'ERROR: Both HTTPS and HTTP failed:' $_.Exception.Message -ForegroundColor Red } }"
echo.

REM Update URLs if HTTPS failed
for /f %%i in ('powershell -Command "if ($global:BASE_URL) { $global:BASE_URL } else { '%BASE_URL%' }"') do set BASE_URL=%%i
for /f %%i in ('powershell -Command "if ($global:API_URL) { $global:API_URL } else { '%API_URL%' }"') do set API_URL=%%i

REM Test 2: Root endpoint
echo 2. Testing Root Endpoint...
echo URL: %BASE_URL%/
echo.
powershell -Command "try { $response = Invoke-RestMethod -Uri '%BASE_URL%/' -Method Get -SkipCertificateCheck; Write-Host 'SUCCESS: Root Response:' -ForegroundColor Green; Write-Host $response } catch { Write-Host 'ERROR: Root endpoint failed:' $_.Exception.Message -ForegroundColor Red }"
echo.

REM Test 3: Create a BUY trade
echo 3. Creating a BUY trade...
echo URL: %API_URL%
echo.
powershell -Command "$body = @{ symbol='AAPL'; quantity=100; price=150.50; tradeType=1; userId='testuser' } | ConvertTo-Json; try { $response = Invoke-RestMethod -Uri '%API_URL%' -Method Post -Body $body -ContentType 'application/json' -SkipCertificateCheck; Write-Host 'SUCCESS: BUY Trade Created:' -ForegroundColor Green; $response | ConvertTo-Json } catch { Write-Host 'ERROR: BUY Trade failed:' $_.Exception.Message -ForegroundColor Red; if ($_.Exception.Response) { $_.Exception.Response.StatusCode } }"
echo.

REM Test 4: Create a SELL trade
echo 4. Creating a SELL trade...
echo.
powershell -Command "$body = @{ symbol='GOOGL'; quantity=50; price=2800.75; tradeType=2; userId='testuser' } | ConvertTo-Json; try { $response = Invoke-RestMethod -Uri '%API_URL%' -Method Post -Body $body -ContentType 'application/json' -SkipCertificateCheck; Write-Host 'SUCCESS: SELL Trade Created:' -ForegroundColor Green; $response | ConvertTo-Json } catch { Write-Host 'ERROR: SELL Trade failed:' $_.Exception.Message -ForegroundColor Red }"
echo.

REM Test 5: Get all trades
echo 5. Getting all trades...
echo URL: %API_URL%
echo.
powershell -Command "try { $response = Invoke-RestMethod -Uri '%API_URL%' -Method Get -SkipCertificateCheck; Write-Host 'SUCCESS: All Trades Retrieved:' -ForegroundColor Green; $response | ConvertTo-Json } catch { Write-Host 'ERROR: Failed to get trades:' $_.Exception.Message -ForegroundColor Red }"
echo.

REM Test 6: Get trades for specific user
echo 6. Getting trades for testuser...
echo URL: %API_URL%?userId=testuser
echo.
powershell -Command "try { $response = Invoke-RestMethod -Uri '%API_URL%?userId=testuser' -Method Get -SkipCertificateCheck; Write-Host 'SUCCESS: User Trades Retrieved:' -ForegroundColor Green; $response | ConvertTo-Json } catch { Write-Host 'ERROR: Failed to get user trades:' $_.Exception.Message -ForegroundColor Red }"
echo.

REM Test 7: Get trade statistics
echo 7. Getting trade statistics for testuser...
echo URL: %API_URL%/statistics/testuser
echo.
powershell -Command "try { $response = Invoke-RestMethod -Uri '%API_URL%/statistics/testuser' -Method Get -SkipCertificateCheck; Write-Host 'SUCCESS: Trade Statistics Retrieved:' -ForegroundColor Green; $response | ConvertTo-Json } catch { Write-Host 'ERROR: Failed to get trade statistics:' $_.Exception.Message -ForegroundColor Red }"
echo.

REM Test 8: Invalid trade (should fail)
echo 8. Testing invalid trade (should fail)...
echo.
powershell -Command "$body = @{ symbol=''; quantity=-100; price=0; tradeType=1; userId='' } | ConvertTo-Json; try { $response = Invoke-RestMethod -Uri '%API_URL%' -Method Post -Body $body -ContentType 'application/json' -SkipCertificateCheck; Write-Host 'ERROR: Invalid trade was accepted (this should not happen)' -ForegroundColor Red; $response | ConvertTo-Json } catch { Write-Host 'SUCCESS: Invalid Trade Properly Rejected:' -ForegroundColor Green; Write-Host $_.Exception.Message }"
echo.

echo ========================================
echo API Testing Completed!
echo ========================================
echo.
echo Manual Testing URLs:
echo Swagger UI: %BASE_URL%
echo Health Check: %BASE_URL%/health  
echo All Trades: %API_URL%
echo User Statistics: %API_URL%/statistics/testuser
echo.
echo If tests failed:
echo 1. Make sure the API is running (dotnet run in TradingService folder)
echo 2. Check if the URL %BASE_URL% is correct
echo 3. Verify the API started successfully
echo 4. If HTTPS fails, try HTTP: %HTTP_URL%
echo 5. Trust dev certificates: dotnet dev-certs https --trust
echo.

pause