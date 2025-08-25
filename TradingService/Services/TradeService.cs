using Microsoft.EntityFrameworkCore;
using TradingService.Data;
using TradingService.Models;
using TradingService.Models.DTOs;
using TradingService.Services.Interfaces;

namespace TradingService.Services
{
    /// <summary>
    /// Service for handling trade operations
    /// </summary>
    public class TradeService : ITradeService
    {
        private readonly TradingDbContext _context;
        private readonly IMessageQueueService _messageQueueService;
        private readonly ILogger<TradeService> _logger;

        public TradeService(
            TradingDbContext context,
            IMessageQueueService messageQueueService,
            ILogger<TradeService> logger)
        {
            _context = context;
            _messageQueueService = messageQueueService;
            _logger = logger;
        }

        /// <summary>
        /// Executes a new trade
        /// </summary>
        /// <param name="createTradeDto">Trade details</param>
        /// <returns>The executed trade</returns>
        public async Task<Trade> ExecuteTradeAsync(CreateTradeDto createTradeDto)
        {
            _logger.LogInformation("Executing trade for user {UserId}, Symbol: {Symbol}, Quantity: {Quantity}", 
                createTradeDto.UserId, createTradeDto.Symbol, createTradeDto.Quantity);

            // Validate required fields
            if (string.IsNullOrWhiteSpace(createTradeDto.Symbol))
                throw new ArgumentException("Symbol is required", nameof(createTradeDto.Symbol));

            if (string.IsNullOrWhiteSpace(createTradeDto.UserId))
                throw new ArgumentException("UserId is required", nameof(createTradeDto.UserId));

            try
            {
                // Create trade entity
                var trade = new Trade
                {
                    Symbol = createTradeDto.Symbol.ToUpper(),
                    Quantity = createTradeDto.Quantity,
                    Price = createTradeDto.Price,
                    TradeType = createTradeDto.TradeType,
                    UserId = createTradeDto.UserId,
                    ExecutedAt = DateTime.UtcNow,
                    Status = TradeStatus.Pending
                };

                // Add to database
                _context.Trades.Add(trade);
                await _context.SaveChangesAsync();

                // Update status to executed
                trade.Status = TradeStatus.Executed;
                await _context.SaveChangesAsync();

                // Publish to message queue
                await _messageQueueService.PublishTradeAsync(trade);

                _logger.LogInformation("Trade executed successfully. TradeId: {TradeId}", trade.Id);

                return trade;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to execute trade for user {UserId}", createTradeDto.UserId);
                throw;
            }
        }

        /// <summary>
        /// Retrieves trades based on query parameters
        /// </summary>
        /// <param name="query">Query parameters</param>
        /// <returns>List of trades and total count</returns>
        public async Task<(IEnumerable<Trade> Trades, int TotalCount)> GetTradesAsync(TradeQueryDto query)
        {
            _logger.LogInformation("Retrieving trades with query parameters");

            var queryable = _context.Trades.AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(query.UserId))
            {
                queryable = queryable.Where(t => t.UserId == query.UserId);
            }

            if (!string.IsNullOrEmpty(query.Symbol))
            {
                queryable = queryable.Where(t => t.Symbol == query.Symbol.ToUpper());
            }

            if (query.TradeType.HasValue)
            {
                queryable = queryable.Where(t => t.TradeType == query.TradeType.Value);
            }

            if (query.FromDate.HasValue)
            {
                queryable = queryable.Where(t => t.ExecutedAt >= query.FromDate.Value);
            }

            if (query.ToDate.HasValue)
            {
                queryable = queryable.Where(t => t.ExecutedAt <= query.ToDate.Value);
            }

            // Get total count
            var totalCount = await queryable.CountAsync();

            // Apply pagination and ordering
            var trades = await queryable
                .OrderByDescending(t => t.ExecutedAt)
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

            _logger.LogInformation("Retrieved {Count} trades out of {Total} total", trades.Count, totalCount);

            return (trades, totalCount);
        }

        /// <summary>
        /// Retrieves a specific trade by ID
        /// </summary>
        /// <param name="id">Trade ID</param>
        /// <returns>The trade if found</returns>
        public async Task<Trade?> GetTradeByIdAsync(int id)
        {
            _logger.LogInformation("Retrieving trade with ID: {TradeId}", id);

            var trade = await _context.Trades.FindAsync(id);

            if (trade == null)
            {
                _logger.LogWarning("Trade with ID {TradeId} not found", id);
            }

            return trade;
        }

        /// <summary>
        /// Gets trade statistics for a user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>Trade statistics</returns>
        public async Task<TradeStatistics> GetTradeStatisticsAsync(string userId)
        {
            _logger.LogInformation("Calculating trade statistics for user: {UserId}", userId);

            var trades = await _context.Trades
                .Where(t => t.UserId == userId && t.Status == TradeStatus.Executed)
                .ToListAsync();

            var statistics = new TradeStatistics
            {
                TotalTrades = trades.Count,
                TotalVolume = trades.Sum(t => t.TotalValue),
                BuyTrades = trades.Count(t => t.TradeType == TradeType.Buy),
                SellTrades = trades.Count(t => t.TradeType == TradeType.Sell),
                AverageTradeValue = trades.Any() ? trades.Average(t => t.TotalValue) : 0
            };

            _logger.LogInformation("Statistics calculated for user {UserId}: {TotalTrades} trades, {TotalVolume} total volume", 
                userId, statistics.TotalTrades, statistics.TotalVolume);

            return statistics;
        }
    }
}