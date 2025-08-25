using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;
using System.Net;
using TradingService.Data;
using TradingService.Models;
using TradingService.Models.DTOs;
using TradingService.Services.Interfaces;
using Xunit;
using FluentAssertions;

namespace TradingService.Tests.Integration
{
    /// <summary>
    /// Integration tests for Trading API endpoints
    /// </summary>
    public class TradingApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;
        private readonly IServiceScope _scope;
        private readonly TradingDbContext _context;

        public TradingApiIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Remove the existing DbContext registration
                    var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<TradingDbContext>));
                    if (descriptor != null)
                        services.Remove(descriptor);

                    // Add in-memory database for testing
                    services.AddDbContext<TradingDbContext>(options =>
                    {
                        options.UseInMemoryDatabase("TestDb");
                    });

                    // Override configuration for testing
                    services.Configure<Microsoft.Extensions.Configuration.IConfiguration>(config =>
                    {
                        config["UseInMemoryDatabase"] = "true";
                    });
                });
            });

            _client = _factory.CreateClient();
            _scope = _factory.Services.CreateScope();
            _context = _scope.ServiceProvider.GetRequiredService<TradingDbContext>();
        }

        #region Trade Execution Tests

        [Fact]
        public async Task POST_ExecuteTrade_ValidTrade_ShouldReturn201()
        {
            // Arrange
            var createTradeDto = new CreateTradeDto
            {
                Symbol = "AAPL",
                Quantity = 100,
                Price = 150.50m,
                TradeType = TradeType.Buy,
                UserId = "integrationtest"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/trades", createTradeDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            
            var tradeDto = await response.Content.ReadFromJsonAsync<TradeDto>();
            tradeDto.Should().NotBeNull();
            tradeDto!.Symbol.Should().Be("AAPL");
            tradeDto.Quantity.Should().Be(100);
            tradeDto.Price.Should().Be(150.50m);
            tradeDto.UserId.Should().Be("integrationtest");

            // Verify trade was saved to database
            var savedTrade = await _context.Trades.FindAsync(tradeDto.Id);
            savedTrade.Should().NotBeNull();
        }

        [Fact]
        public async Task POST_ExecuteTrade_InvalidTrade_ShouldReturn400()
        {
            // Arrange
            var invalidTradeDto = new CreateTradeDto
            {
                Symbol = "", // Invalid
                Quantity = -1, // Invalid
                Price = 0, // Invalid
                TradeType = TradeType.Buy,
                UserId = "" // Invalid
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/trades", invalidTradeDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        #endregion

        #region Trade Retrieval Tests

        [Fact]
        public async Task GET_GetTrades_ShouldReturnAllTrades()
        {
            // Arrange
            await SeedTestData();

            // Act
            var response = await _client.GetAsync("/api/trades");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("data");
            content.Should().Contain("pagination");
        }

        [Fact]
        public async Task GET_GetTrades_WithUserId_ShouldReturnUserTrades()
        {
            // Arrange
            await SeedTestData();

            // Act
            var response = await _client.GetAsync("/api/trades?userId=user1");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("user1");
        }

        [Fact]
        public async Task GET_GetTradeById_ExistingId_ShouldReturn200()
        {
            // Arrange
            await SeedTestData();
            var existingTrade = await _context.Trades.FirstAsync();

            // Act
            var response = await _client.GetAsync($"/api/trades/{existingTrade.Id}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var tradeDto = await response.Content.ReadFromJsonAsync<TradeDto>();
            tradeDto.Should().NotBeNull();
            tradeDto!.Id.Should().Be(existingTrade.Id);
        }

        [Fact]
        public async Task GET_GetTradeById_NonExistingId_ShouldReturn404()
        {
            // Arrange
            var nonExistingId = 99999;

            // Act
            var response = await _client.GetAsync($"/api/trades/{nonExistingId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GET_GetTradeStatistics_ValidUserId_ShouldReturn200()
        {
            // Arrange
            await SeedTestData();

            // Act
            var response = await _client.GetAsync("/api/trades/statistics/user1");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var statistics = await response.Content.ReadFromJsonAsync<TradeStatistics>();
            statistics.Should().NotBeNull();
            statistics!.TotalTrades.Should().BeGreaterThan(0);
        }

        #endregion

        #region Health Check Tests

        [Fact]
        public async Task GET_HealthCheck_ShouldReturn200()
        {
            // Act
            var response = await _client.GetAsync("/health");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        #endregion

        #region Helper Methods

        private async Task SeedTestData()
        {
            // Clear existing data
            _context.Trades.RemoveRange(_context.Trades);
            await _context.SaveChangesAsync();

            var trades = new List<Trade>
            {
                new Trade
                {
                    Symbol = "AAPL",
                    Quantity = 100,
                    Price = 150m,
                    TradeType = TradeType.Buy,
                    UserId = "user1",
                    ExecutedAt = DateTime.UtcNow.AddMinutes(-30),
                    Status = TradeStatus.Executed
                },
                new Trade
                {
                    Symbol = "GOOGL",
                    Quantity = 50,
                    Price = 2800m,
                    TradeType = TradeType.Sell,
                    UserId = "user2",
                    ExecutedAt = DateTime.UtcNow.AddMinutes(-10),
                    Status = TradeStatus.Executed
                }
            };

            _context.Trades.AddRange(trades);
            await _context.SaveChangesAsync();
        }

        #endregion

        public void Dispose()
        {
            _scope.Dispose();
            _context.Dispose();
        }
    }
}