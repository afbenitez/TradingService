#!/bin/bash

echo "Setting up Trading Service Database..."

# Check if PostgreSQL is running
if ! pg_isready -h localhost -p 5432 > /dev/null 2>&1; then
    echo "PostgreSQL is not running. Please start PostgreSQL first."
    echo "You can use: docker-compose up postgres -d"
    exit 1
fi

# Navigate to the TradingService directory
cd TradingService

echo "Creating database migration..."
dotnet ef migrations add InitialCreate --output-dir Data/Migrations

echo "Updating database..."
dotnet ef database update

echo "Database setup completed successfully!"
echo ""
echo "Database connection details:"
echo "Host: localhost"
echo "Port: 5432" 
echo "Database: TradingServiceDb"
echo "Username: postgres"
echo "Password: postgres"