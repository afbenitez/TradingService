# Trading Service - PowerShell Setup Script
# This script handles all setup and execution tasks for Windows

param(
    [string]$Action = "menu"
)

# Colors for PowerShell
$Red = "Red"
$Green = "Green"
$Yellow = "Yellow"
$Blue = "Cyan"

function Write-ColorText {
    param([string]$Text, [string]$Color = "White")
    Write-Host $Text -ForegroundColor $Color
}

function Show-Menu {
    Clear-Host
    Write-ColorText "?? Trading Service - Windows Setup" $Blue
    Write-Host ""
    Write-ColorText "?? Available Operations:" $Yellow
    Write-Host ""
    Write-Host "1. ???  Setup Database"
    Write-Host "2. ?? Start Trading Service"
    Write-Host "3. ?? Test API"
    Write-Host "4. ?? Start Infrastructure (Docker)"
    Write-Host "5. ?? Complete Setup (All-in-one)"
    Write-Host "6. ?? Clean & Reset"
    Write-Host "7. ? Exit"
    Write-Host ""
}

function Test-Docker {
    try {
        docker version | Out-Null
        return $true
    }
    catch {
        return $false
    }
}

function Test-DotNet {
    try {
        dotnet --version | Out-Null
        return $true
    }
    catch {
        return $false
    }
}

function Start-Infrastructure {
    Write-ColorText "?? Starting Docker infrastructure..." $Yellow
    
    if (-not (Test-Docker)) {
        Write-ColorText "? Docker is not running. Please start Docker Desktop first." $Red
        return $false
    }
    
    docker-compose up postgres rabbitmq -d
    
    if ($LASTEXITCODE -eq 0) {
        Write-ColorText "? Infrastructure started successfully" $Green
        Write-Host ""
        Write-ColorText "?? RabbitMQ Management: http://localhost:15672 (guest/guest)" $Blue
        Write-ColorText "??? Database Admin: http://localhost:8080" $Blue
        return $true
    }
    else {
        Write-ColorText "? Failed to start infrastructure" $Red
        return $false
    }
}

function Setup-Database {
    Write-ColorText "??? Setting up database..." $Yellow
    
    if (-not (Test-DotNet)) {
        Write-ColorText "? .NET SDK not found. Please install .NET 8 SDK first." $Red
        return $false
    }
    
    # Wait for PostgreSQL to be ready
    Write-ColorText "? Waiting for PostgreSQL to be ready..." $Yellow
    Start-Sleep -Seconds 10
    
    # Check if migrations exist
    if (-not (Test-Path "TradingService/Data/Migrations")) {
        Write-ColorText "Creating database migration..." $Yellow
        Set-Location "TradingService"
        dotnet ef migrations add InitialCreate --output-dir Data/Migrations
        
        if ($LASTEXITCODE -ne 0) {
            Write-ColorText "? Failed to create migration" $Red
            Set-Location ".."
            return $false
        }
        
        Write-ColorText "Updating database..." $Yellow
        dotnet ef database update
        
        if ($LASTEXITCODE -ne 0) {
            Write-ColorText "? Failed to update database" $Red
            Set-Location ".."
            return $false
        }
        
        Set-Location ".."
        Write-ColorText "? Database setup completed" $Green
    }
    else {
        Write-ColorText "? Database already configured" $Green
    }
    
    return $true
}

function Start-TradingService {
    Write-ColorText "?? Starting Trading Service..." $Yellow
    
    if (-not (Test-DotNet)) {
        Write-ColorText "? .NET SDK not found. Please install .NET 8 SDK first." $Red
        return
    }
    
    Set-Location "TradingService"
    
    Write-ColorText "?? Building project..." $Yellow
    dotnet build
    
    if ($LASTEXITCODE -ne 0) {
        Write-ColorText "? Build failed" $Red
        Set-Location ".."
        return
    }
    
    Write-ColorText "? Build successful" $Green
    Write-Host ""
    Write-ColorText "?? Starting Trading Service..." $Green
    Write-Host ""
    Write-ColorText "?? Service URLs:" $Blue
    Write-ColorText "?? API: https://localhost:7179" $Blue
    Write-ColorText "?? Swagger: https://localhost:7179" $Blue
    Write-ColorText "?? Health: https://localhost:7179/health" $Blue
    Write-Host ""
    Write-ColorText "Press Ctrl+C to stop the service" $Yellow
    Write-Host ""
    
    dotnet run
    Set-Location ".."
}

function Test-API {
    Write-ColorText "?? Testing Trading Service API..." $Yellow
    
    $baseUrl = "https://localhost:7179"
    $apiUrl = "$baseUrl/api/trades"
    
    # Test Health Check
    Write-ColorText "1. Testing Health Check..." $Yellow
    try {
        $response = Invoke-RestMethod -Uri "$baseUrl/health" -Method Get -SkipCertificateCheck
        Write-ColorText "? Health Check: $response" $Green
    }
    catch {
        Write-ColorText "? Health Check failed: $($_.Exception.Message)" $Red
    }
    
    Write-Host ""
    
    # Test Create Trade
    Write-ColorText "2. Creating a BUY trade..." $Yellow
    $tradeData = @{
        symbol = "AAPL"
        quantity = 100
        price = 150.50
        tradeType = 1
        userId = "testuser"
    } | ConvertTo-Json
    
    try {
        $response = Invoke-RestMethod -Uri $apiUrl -Method Post -Body $tradeData -ContentType "application/json" -SkipCertificateCheck
        Write-ColorText "? BUY Trade Created:" $Green
        $response | ConvertTo-Json | Write-Host
    }
    catch {
        Write-ColorText "? BUY Trade creation failed: $($_.Exception.Message)" $Red
    }
    
    Write-Host ""
    
    # Test Get Trades
    Write-ColorText "3. Getting all trades..." $Yellow
    try {
        $response = Invoke-RestMethod -Uri $apiUrl -Method Get -SkipCertificateCheck
        Write-ColorText "? Trades Retrieved:" $Green
        $response | ConvertTo-Json | Write-Host
    }
    catch {
        Write-ColorText "? Failed to get trades: $($_.Exception.Message)" $Red
    }
    
    Write-Host ""
    Write-ColorText "?? Manual Testing URLs:" $Blue
    Write-ColorText "?? Swagger UI: $baseUrl" $Blue
    Write-ColorText "?? Health Check: $baseUrl/health" $Blue
    Write-ColorText "?? All Trades: $apiUrl" $Blue
}

function Complete-Setup {
    Write-ColorText "?? Complete setup starting..." $Blue
    Write-Host ""
    
    # Step 1: Start infrastructure
    Write-ColorText "Step 1/4: Starting infrastructure..." $Yellow
    if (-not (Start-Infrastructure)) {
        return
    }
    
    # Step 2: Setup database
    Write-ColorText "Step 2/4: Setting up database..." $Yellow
    if (-not (Setup-Database)) {
        return
    }
    
    # Step 3: Build and start service
    Write-ColorText "Step 3/4: Building and starting service..." $Yellow
    Start-TradingService
}

function Clean-Reset {
    Write-ColorText "?? Cleaning and resetting..." $Yellow
    
    Write-ColorText "Stopping all containers..." $Yellow
    docker-compose down
    
    Write-ColorText "Removing volumes (this will delete the database)..." $Red
    $confirm = Read-Host "Are you sure you want to delete all data? (y/N)"
    if ($confirm -eq "y" -or $confirm -eq "Y") {
        docker-compose down -v
        Write-ColorText "? Cleanup completed" $Green
    }
    else {
        Write-ColorText "Cleanup cancelled" $Yellow
    }
}

# Main execution
switch ($Action.ToLower()) {
    "infrastructure" { Start-Infrastructure; Read-Host "Press Enter to continue" }
    "database" { Setup-Database; Read-Host "Press Enter to continue" }
    "service" { Start-TradingService }
    "test" { Test-API; Read-Host "Press Enter to continue" }
    "setup" { Complete-Setup }
    "clean" { Clean-Reset; Read-Host "Press Enter to continue" }
    default {
        do {
            Show-Menu
            $choice = Read-Host "Select an option (1-7)"
            
            switch ($choice) {
                "1" { Setup-Database; Read-Host "Press Enter to continue" }
                "2" { Start-TradingService }
                "3" { Test-API; Read-Host "Press Enter to continue" }
                "4" { Start-Infrastructure; Read-Host "Press Enter to continue" }
                "5" { Complete-Setup }
                "6" { Clean-Reset; Read-Host "Press Enter to continue" }
                "7" { Write-ColorText "?? Goodbye!" $Blue; exit }
                default { Write-ColorText "Invalid choice. Please select 1-7." $Red; Start-Sleep 2 }
            }
        } while ($choice -ne "7")
    }
}