using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using TradingService.Data;
using TradingService.Models;
using TradingService.Models.DTOs;
using TradingService.Services;
using TradingService.Services.Interfaces;
using Xunit;
using FluentAssertions;

namespace TradingService.Tests.Services
{
    /// <summary>
    /// Unit tests for TradeService
    /// </summary>
    public class TradeServiceTests : IDisposable
    {
        private readonly TradingDbContext _context;
        private readonly Mock<IMessageQueueService> _mockMessageQueueService;
        private readonly Mock<ILogger<TradeService>> _mockLogger;
        private readonly TradeService _tradeService;

        public TradeServiceTests()
        {
            // Setup in-memory database for testing
            var options = new DbContextOptionsBuilder<TradingDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new TradingDbContext(options);
            _mockMessageQueueService = new Mock<IMessageQueueService>();
            _mockLogger = new Mock<ILogger<TradeService>>();

            _tradeService = new TradeService(_context, _mockMessageQueueService.Object, _mockLogger.Object);
        }

        #region ExecuteTradeAsync Tests

        [Fact]
        public async Task ExecuteTradeAsync_ValidTrade_ShouldCreateTrade()
        {
            // Arrange
            var createTradeDto = new CreateTradeDto
            {
                Symbol = "AAPL",
                Quantity = 100,
                Price = 150.50m,
                TradeType = TradeType.Buy,
                UserId = "testuser"
            };

            // Act
            var result = await _tradeService.ExecuteTradeAsync(createTradeDto);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().BeGreaterThan(0);
            result.Symbol.Should().Be("AAPL");
            result.Quantity.Should().Be(100);
            result.Price.Should().Be(150.50m);
            result.TradeType.Should().Be(TradeType.Buy);
            result.UserId.Should().Be("testuser");
            result.Status.Should().Be(TradeStatus.Executed);
            result.TotalValue.Should().Be(15050m); // 100 * 150.50

            // Verify trade was saved to database
            var savedTrade = await _context.Trades.FindAsync(result.Id);
            savedTrade.Should().NotBeNull();
            savedTrade!.Symbol.Should().Be("AAPL");

            // Verify message was published
            _mockMessageQueueService.Verify(x => x.PublishTradeAsync(It.IsAny<Trade>()), Times.Once);
        }

        [Fact]
        public async Task ExecuteTradeAsync_SymbolToLowerCase_ShouldConvertToUpperCase()
        {
            // Arrange
            var createTradeDto = new CreateTradeDto
            {
                Symbol = "aapl",
                Quantity = 50,
                Price = 100m,
                TradeType = TradeType.Sell,
                UserId = "testuser"
            };

            // Act
            var result = await _tradeService.ExecuteTradeAsync(createTradeDto);

            // Assert
            result.Symbol.Should().Be("AAPL");
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public async Task ExecuteTradeAsync_InvalidSymbol_ShouldThrowArgumentException(string invalidSymbol)
        {
            // Arrange
            var createTradeDto = new CreateTradeDto
            {
                Symbol = invalidSymbol!,
                Quantity = 100,
                Price = 150.50m,
                TradeType = TradeType.Buy,
                UserId = "testuser"
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _tradeService.ExecuteTradeAsync(createTradeDto));
            exception.Message.Should().Contain("Symbol is required");
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public async Task ExecuteTradeAsync_InvalidUserId_ShouldThrowArgumentException(string invalidUserId)
        {
            // Arrange
            var createTradeDto = new CreateTradeDto
            {
                Symbol = "AAPL",
                Quantity = 100,
                Price = 150.50m,
                TradeType = TradeType.Buy,
                UserId = invalidUserId!
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _tradeService.ExecuteTradeAsync(createTradeDto));
            exception.Message.Should().Contain("UserId is required");
        }

        [Fact]
        public async Task ExecuteTradeAsync_MessageQueueThrows_ShouldStillSaveTrade()
        {
            // Arrange
            var createTradeDto = new CreateTradeDto
            {
                Symbol = "AAPL",
                Quantity = 100,
                Price = 150.50m,
                TradeType = TradeType.Buy,
                UserId = "testuser"
            };

            _mockMessageQueueService.Setup(x => x.PublishTradeAsync(It.IsAny<Trade>()))
                .ThrowsAsync(new Exception("Message queue error"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _tradeService.ExecuteTradeAsync(createTradeDto));
            exception.Message.Should().Contain("Message queue error");

            // Trade should still be saved despite message queue failure
            var trades = await _context.Trades.ToListAsync();
            trades.Should().HaveCount(1);
        }

        #endregion

        #region GetTradesAsync Tests

        [Fact]
        public async Task GetTradesAsync_WithoutFilters_ShouldReturnAllTrades()
        {
            // Arrange
            await SeedTestData();
            var query = new TradeQueryDto { Page = 1, PageSize = 10 };

            // Act
            var (trades, totalCount) = await _tradeService.GetTradesAsync(query);

            // Assert
            totalCount.Should().Be(3);
            trades.Should().HaveCount(3);
            trades.Should().BeInDescendingOrder(t => t.ExecutedAt);
        }

        [Fact]
        public async Task GetTradesAsync_FilterByUserId_ShouldReturnUserTrades()
        {
            // Arrange
            await SeedTestData();
            var query = new TradeQueryDto { UserId = "user1", Page = 1, PageSize = 10 };

            // Act
            var (trades, totalCount) = await _tradeService.GetTradesAsync(query);

            // Assert
            totalCount.Should().Be(2);
            trades.Should().HaveCount(2);
            trades.Should().OnlyContain(t => t.UserId == "user1");
        }

        [Fact]
        public async Task GetTradesAsync_FilterBySymbol_ShouldReturnSymbolTrades()
        {
            // Arrange
            await SeedTestData();
            var query = new TradeQueryDto { Symbol = "AAPL", Page = 1, PageSize = 10 };

            // Act
            var (trades, totalCount) = await _tradeService.GetTradesAsync(query);

            // Assert
            totalCount.Should().Be(2);
            trades.Should().HaveCount(2);
            trades.Should().OnlyContain(t => t.Symbol == "AAPL");
        }

        [Fact]
        public async Task GetTradesAsync_FilterByTradeType_ShouldReturnFilteredTrades()
        {
            // Arrange
            await SeedTestData();
            var query = new TradeQueryDto { TradeType = TradeType.Buy, Page = 1, PageSize = 10 };

            // Act
            var (trades, totalCount) = await _tradeService.GetTradesAsync(query);

            // Assert
            totalCount.Should().Be(2);
            trades.Should().HaveCount(2);
            trades.Should().OnlyContain(t => t.TradeType == TradeType.Buy);
        }

        [Fact]
        public async Task GetTradesAsync_WithPagination_ShouldReturnCorrectPage()
        {
            // Arrange
            await SeedTestData();
            var query = new TradeQueryDto { Page = 2, PageSize = 1 };

            // Act
            var (trades, totalCount) = await _tradeService.GetTradesAsync(query);

            // Assert
            totalCount.Should().Be(3);
            trades.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetTradesAsync_FilterByDateRange_ShouldReturnFilteredTrades()
        {
            // Arrange
            await SeedTestData();
            var fromDate = DateTime.UtcNow.AddDays(-1);
            var toDate = DateTime.UtcNow;
            var query = new TradeQueryDto { FromDate = fromDate, ToDate = toDate, Page = 1, PageSize = 10 };

            // Act
            var (trades, totalCount) = await _tradeService.GetTradesAsync(query);

            // Assert
            totalCount.Should().BeGreaterThan(0);
            trades.Should().OnlyContain(t => t.ExecutedAt >= fromDate && t.ExecutedAt <= toDate);
        }

        #endregion

        #region GetTradeByIdAsync Tests

        [Fact]
        public async Task GetTradeByIdAsync_ExistingId_ShouldReturnTrade()
        {
            // Arrange
            await SeedTestData();
            var existingTrade = await _context.Trades.FirstAsync();

            // Act
            var result = await _tradeService.GetTradeByIdAsync(existingTrade.Id);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(existingTrade.Id);
            result.Symbol.Should().Be(existingTrade.Symbol);
        }

        [Fact]
        public async Task GetTradeByIdAsync_NonExistingId_ShouldReturnNull()
        {
            // Arrange
            var nonExistingId = 99999;

            // Act
            var result = await _tradeService.GetTradeByIdAsync(nonExistingId);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region GetTradeStatisticsAsync Tests

        [Fact]
        public async Task GetTradeStatisticsAsync_ValidUserId_ShouldReturnCorrectStatistics()
        {
            // Arrange
            await SeedTestData();

            // Act
            var statistics = await _tradeService.GetTradeStatisticsAsync("user1");

            // Assert
            statistics.Should().NotBeNull();
            statistics.TotalTrades.Should().Be(2);
            statistics.BuyTrades.Should().Be(2);
            statistics.SellTrades.Should().Be(0);
            statistics.TotalVolume.Should().Be(25100m); // (100*150) + (50*202) = 15000 + 10100 = 25100
            statistics.AverageTradeValue.Should().Be(12550m); // 25100/2 = 12550
        }

        [Fact]
        public async Task GetTradeStatisticsAsync_UserWithNoTrades_ShouldReturnZeroStatistics()
        {
            // Arrange
            await SeedTestData();

            // Act
            var statistics = await _tradeService.GetTradeStatisticsAsync("nonexistentuser");

            // Assert
            statistics.Should().NotBeNull();
            statistics.TotalTrades.Should().Be(0);
            statistics.BuyTrades.Should().Be(0);
            statistics.SellTrades.Should().Be(0);
            statistics.TotalVolume.Should().Be(0);
            statistics.AverageTradeValue.Should().Be(0);
        }

        #endregion

        #region Helper Methods

        private async Task SeedTestData()
        {
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
                    Symbol = "AAPL",
                    Quantity = 50,
                    Price = 202m,
                    TradeType = TradeType.Buy,
                    UserId = "user1",
                    ExecutedAt = DateTime.UtcNow.AddMinutes(-20),
                    Status = TradeStatus.Executed
                },
                new Trade
                {
                    Symbol = "GOOGL",
                    Quantity = 25,
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
            _context.Dispose();
        }
    }
}