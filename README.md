# MeDirect Trading Service - Banking Microservice

[![.NET 8](https://img.shields.io/badge/.NET-8.0-blue.svg)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-15-blue.svg)](https://www.postgresql.org/)
[![RabbitMQ](https://img.shields.io/badge/RabbitMQ-3-orange.svg)](https://www.rabbitmq.com/)
[![Docker](https://img.shields.io/badge/Docker-Supported-blue.svg)](https://www.docker.com/)
[![Tests](https://img.shields.io/badge/Tests-xUnit-green.svg)](https://xunit.net/)

> **A production-ready microservice for trading operations, built as part of MeDirect's transition from monolith to microservices architecture.**

## **Project Overview**

This microservice handles all trading functionality for the bank, providing:
- **Trade Execution** via RESTful APIs  
- **Trade Retrieval** with advanced filtering and pagination
- **Data Persistence** in PostgreSQL database
- **Message Queue Integration** with RabbitMQ for async notifications
- **Console Application** demonstrating message consumption
- **Comprehensive Unit & Integration Tests**
- **Production-ready Logging** with Serilog
- **Full Docker Support** for containerized deployment

## **Architecture**

```
+------------------+    +------------------+    +------------------+
|   Trading API    |--->|   PostgreSQL     |    |    RabbitMQ      |
|   (REST API)     |    |   (Database)     |    | (Message Queue)  |
|   .NET 8         |    |   Persistent     |    |   Async Msgs     |
+------------------+    +------------------+    +------------------+
                                                         |
                                                         v
                                                +------------------+
                                                | Trading Consumer |
                                                | (Console App)    |
                                                | Message Logger   |
                                                +------------------+
```

## **Technologies Used**

| Category | Technology | Version | Purpose |
|----------|------------|---------|---------|
| **Framework** | .NET 8 | 8.0 | Main application framework |
| **Web API** | ASP.NET Core | 8.0 | RESTful API implementation |
| **Database** | PostgreSQL | 15 | Persistent data storage |
| **ORM** | Entity Framework Core | 8.0 | Database access and migrations |
| **Message Queue** | RabbitMQ | 3-management | Asynchronous messaging |
| **Logging** | Serilog | 7.0 | Structured logging |
| **Validation** | FluentValidation | 11.8 | Input validation |
| **Mapping** | AutoMapper | 12.0 | Object-to-object mapping |
| **Testing** | xUnit + Moq + FluentAssertions | Latest | Unit and integration testing |
| **Documentation** | Swagger/OpenAPI | 6.6 | API documentation |
| **Containerization** | Docker + Docker Compose | Latest | Deployment and orchestration |

## **Database Configuration**

### **Default: PostgreSQL (Production-Ready)**
```json
{
  "UseInMemoryDatabase": false,
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=TradingServiceDb;Username=postgres;Password=postgres;"
  }
}
```

### **Alternative: In-Memory Database (Testing)**
```json
{
  "UseInMemoryDatabase": true
}
```

**Easy Switch Options:**
1. **Configuration**: Set `"UseInMemoryDatabase": true` in appsettings.json
2. **Environment Variable**: `UseInMemoryDatabase=true`
3. **Code Override**: Uncomment lines in Program.cs

## **Prerequisites**

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- [Git](https://git-scm.com/downloads)

## **Quick Start**

### **Option 1: Automated Setup (Recommended)**
```cmd
# Windows - Complete setup
setup-database-complete.bat

# This will:
# 1. Start PostgreSQL and Adminer
# 2. Initialize the database
# 3. Guide you through accessing the database
```

### **Option 2: Manual Steps**
```cmd
# 1. Start infrastructure
docker-compose up postgres rabbitmq adminer -d

# 2. Run the API
cd TradingService
dotnet run

# 3. Run the consumer (optional)
cd TradingConsumer  
dotnet run
```

## **Testing**

### **Comprehensive Test Suite**
- **Unit Tests** - Services, Validators, Business Logic (30+ tests)
- **Integration Tests** - End-to-end API testing (10+ tests)  
- **Validation Tests** - Input validation and business rules (15+ tests)
- **Mocking** - External dependencies (RabbitMQ, Database)
- **Test Coverage** - Comprehensive coverage of critical paths

### **Running Tests - Multiple Options**

#### **Option 1: Automated Test Runner (Recommended)**
```cmd
# Run the automated test script
run-tests.bat

# Interactive menu with options:
# 1. Run all tests
# 2. Run unit tests only  
# 3. Run integration tests only
# 4. Run with coverage report
# 5. Run specific test class
```

#### **Option 2: Manual Commands**
```cmd
cd TradingService.Tests

# All tests (55+ tests)
dotnet test

# Unit tests only (Services + Validators)
dotnet test --filter "FullyQualifiedName~Services|FullyQualifiedName~Validators"

# Integration tests only  
dotnet test --filter "FullyQualifiedName~Integration"

# With detailed output
dotnet test --verbosity normal

# With coverage report
dotnet test --collect:"XPlat Code Coverage"

# Continuous testing (reruns on file changes)
dotnet watch test
```

#### **Option 3: Visual Studio**
- **Test Explorer**: View -> Test Explorer
- **Run All**: Ctrl+R, A
- **Debug Tests**: Right-click -> Debug

### **Test Categories & Coverage**

| Test Category | Count | Purpose | Coverage |
|---------------|-------|---------|----------|
| **TradeServiceTests** | 15+ | Core business logic testing | Service layer, database operations |
| **CreateTradeDtoValidatorTests** | 15+ | Input validation testing | All validation rules, edge cases |
| **TradingApiIntegrationTests** | 10+ | End-to-end API testing | Complete request/response cycle |
| **Edge Case Tests** | 15+ | Error handling, boundary conditions | Exception scenarios |

### **Test Results - Expected Output**
```
Test run for TradingService.Tests.dll (.NET 8.0)
Microsoft (R) Test Execution Command Line Tool Version 17.8.0
Starting test execution, please wait...
A total of 1 test files matched the specified pattern.

Passed!  - Failed:     0, Passed:    55, Skipped:     0, Total:    55, Duration: 5s
```

### **What Gets Tested**
- **Trade Creation** - Valid/invalid trade scenarios
- **Trade Retrieval** - Filtering, pagination, user-specific queries
- **Trade Statistics** - Calculation accuracy, edge cases
- **Input Validation** - Symbol, quantity, price, user ID validation
- **Error Handling** - Database failures, RabbitMQ failures
- **API Endpoints** - HTTP status codes, response formats
- **Business Rules** - Trade value limits, data integrity
- **Message Queue** - Verify trade messages are published

## **API Endpoints**

### **Execute Trade**
```http
POST /api/trades
Content-Type: application/json

{
  "symbol": "AAPL",
  "quantity": 100,
  "price": 150.50,
  "tradeType": 1,
  "userId": "trader123"
}
```

### **Get Trades**
```http
GET /api/trades?userId=trader123&page=1&pageSize=10
GET /api/trades/{id}
GET /api/trades/statistics/{userId}
```

### **Health & Monitoring**
```http
GET /health
GET / (Swagger UI)
```

## **API Testing**

### **Automated API Testing**
```cmd
# Test all API endpoints with sample data
test-api.bat

# Tests include:
# - Health check
# - Trade creation (BUY/SELL)
# - Trade retrieval  
# - Statistics calculation
# - Error scenarios
```

### **Manual Testing via Swagger**
1. Start the API: `cd TradingService && dotnet run`
2. Open browser: `http://localhost:5100`
3. Use Swagger UI to test endpoints interactively

## **Database Schema**

### **Trades Table**
| Column | Type | Description |
|--------|------|-------------|
| id | int | Primary key |
| symbol | varchar(10) | Stock symbol (e.g., AAPL) |
| quantity | int | Number of shares |
| price | decimal(18,2) | Price per share |
| trade_type | int | 1=Buy, 2=Sell |
| executed_at | timestamp | Execution time |
| user_id | varchar(50) | User identifier |
| status | int | Trade status |

### **Database Access & Verification**
```cmd
# Start database services
start-database.bat

# Access via web interface
# URL: http://localhost:8080
# System: PostgreSQL
# Server: postgres
# Username: postgres  
# Password: postgres
# Database: TradingServiceDb

# Useful queries:
# SELECT * FROM trades;
# SELECT * FROM trades WHERE user_id = 'testuser';
# SELECT COUNT(*) FROM trades;
```

## **Logging Strategy**

### **Structured Logging with Serilog**
- **Console Output** - Development visibility
- **File Rotation** - Daily log files in `/logs`
- **Contextual Enrichment** - User, Trade, and Request tracking
- **Log Levels** - Debug, Information, Warning, Error, Fatal

### **Logged Events**
- **Trade Execution** - Complete trade lifecycle
- **API Requests** - All HTTP requests/responses
- **Database Operations** - EF Core query logging
- **Message Queue** - RabbitMQ publish/consume events
- **Application Lifecycle** - Startup, shutdown, errors

**Note**: This implements **technical logging** (Serilog). For **user authentication/authorization**, implement JWT tokens, OAuth2, or similar as needed.

## **Docker Deployment**

### **Development Environment**
```cmd
# Quick database setup
start-database.bat

# Or manual
docker-compose up postgres rabbitmq adminer -d
```

### **Full Production Deployment**
```cmd
# Complete stack
docker-compose up --build
```

### **Service URLs (Docker)**
- **Trading API**: http://localhost:8000
- **Swagger UI**: http://localhost:8000
- **Database Admin**: http://localhost:8080
- **RabbitMQ Management**: http://localhost:15672

## **Monitoring & Health Checks**

### **Health Endpoints**
- **API Health**: `/health`
- **Database**: Automatic EF Core health check
- **RabbitMQ**: Connection and queue health check

### **Metrics Available**
- Trade execution rates
- Database connection status
- Message queue depth
- API response times

## **Configuration Management**

### **Environment-Specific Settings**
- **Development**: `appsettings.Development.json`
- **Production**: `appsettings.json`
- **Docker**: Environment variables in `docker-compose.yml`

### **Key Configuration Options**
```json
{
  "UseInMemoryDatabase": false,
  "ConnectionStrings": {
    "DefaultConnection": "...",
    "RabbitMQ": "..."
  },
  "RabbitMQ": {
    "HostName": "localhost",
    "Exchange": "trading_exchange",
    "Queue": "trades"
  }
}
```

## **Project Structure**

```
TradingService/
├── TradingService/              # Main API project
│   ├── Controllers/             # API controllers
│   ├── Data/                   # Database context & migrations
│   ├── Models/                 # Domain models & DTOs
│   ├── Services/               # Business logic services
│   ├── Mappings/               # AutoMapper profiles
│   ├── Validators/             # FluentValidation validators
│   ├── Program.cs              # Application entry point
│   └── Dockerfile              # API containerization
│
├── TradingConsumer/            # Message consumer app
│   ├── Services/               # Consumer services
│   ├── Program.cs              # Consumer entry point
│   └── Dockerfile              # Consumer containerization
│
├── TradingService.Tests/       # Test project (55+ tests)
│   ├── Services/               # Service unit tests
│   ├── Validators/             # Validator tests
│   ├── Integration/            # Integration tests
│   └── TradingService.Tests.csproj # Test project file
│
├── docker-compose.yml          # Multi-container orchestration
├── run-tests.bat              # Automated test runner
├── test-api.bat               # API testing script
├── start-database.bat         # Database setup script
├── fix-database-access.bat    # Database troubleshooting
└── README.md                  # This documentation
```

## **Getting Started Checklist**

- [ ] **Clone Repository** - `git clone <repo-url>`
- [ ] **Install Prerequisites** - .NET 8 SDK, Docker Desktop
- [ ] **Start Database** - `start-database.bat`
- [ ] **Run Tests** - `run-tests.bat` (verify all pass)
- [ ] **Start API** - `cd TradingService && dotnet run`
- [ ] **Test API** - `test-api.bat` or visit Swagger UI
- [ ] **Verify Database** - Access Adminer at http://localhost:8080

## **Business Requirements Compliance**

| Requirement | Implementation | Status |
|-------------|----------------|--------|
| Execute trades via API | POST /api/trades | ✅ COMPLETE |
| Retrieve trades via API | GET /api/trades + filtering | ✅ COMPLETE |
| Retain trade information | PostgreSQL with EF Core | ✅ COMPLETE |
| Message queue on trade execution | RabbitMQ integration | ✅ COMPLETE |
| Console app for message logging | TradingConsumer project | ✅ COMPLETE |
| C# .NET Core 6.0+ | .NET 8 (latest) | ✅ COMPLETE |
| Entity Framework Core | EF Core 8.0 | ✅ COMPLETE |
| Database implementation | PostgreSQL (production) | ✅ COMPLETE |
| RESTful APIs | ASP.NET Core Web API | ✅ COMPLETE |
| Unit Tests | xUnit with 55+ comprehensive tests | ✅ COMPLETE |
| Logging | Serilog structured logging | ✅ COMPLETE |
| Docker | Complete containerization | ✅ COMPLETE |

## **Security Considerations**

- **Input Validation** - FluentValidation on all inputs
- **SQL Injection Protection** - Entity Framework parameterized queries
- **CORS Policy** - Configurable cross-origin policies  
- **Error Handling** - Safe error responses without sensitive data
- **Health Checks** - Non-sensitive system status information

**Note**: Authentication/Authorization not implemented - add JWT/OAuth2 as needed for production.

## **Production Deployment Considerations**

### **Environment Variables**
```bash
# Database
DATABASE_CONNECTION_STRING="Host=prod-db;Database=TradingServiceDb;..."
USE_IN_MEMORY_DATABASE=false

# Message Queue  
RABBITMQ_CONNECTION_STRING="amqp://user:pass@prod-rabbitmq:5672"

# Logging
ASPNETCORE_ENVIRONMENT=Production
```

### **Scaling & Performance**
- **Database Indexing** - Optimized queries on user_id, symbol, executed_at
- **Connection Pooling** - Built-in EF Core connection pooling
- **Message Queue Durability** - Persistent RabbitMQ queues
- **Health Checks** - Ready for load balancer integration

## **Support & Contact**

- **Technical Issues**: Create GitHub issue
- **Email**: af.benitez@uniandes.edu.co  
- **Documentation**: See README.md and code comments
- **API Testing**: Use included Swagger UI

## **License**

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

**Built with ❤️ for MeDirect's microservices transformation journey**