using TradingService.Models;
using TradingService.Models.DTOs;

namespace TradingService.Services.Interfaces
{
    /// <summary>
    /// Interface for trade service operations
    /// </summary>
    public interface ITradeService
    {
        /// <summary>
        /// Executes a new trade
        /// </summary>
        /// <param name="createTradeDto">Trade details</param>
        /// <returns>The executed trade</returns>
        Task<Trade> ExecuteTradeAsync(CreateTradeDto createTradeDto);

        /// <summary>
        /// Retrieves trades based on query parameters
        /// </summary>
        /// <param name="query">Query parameters</param>
        /// <returns>List of trades</returns>
        Task<(IEnumerable<Trade> Trades, int TotalCount)> GetTradesAsync(TradeQueryDto query);

        /// <summary>
        /// Retrieves a specific trade by ID
        /// </summary>
        /// <param name="id">Trade ID</param>
        /// <returns>The trade if found</returns>
        Task<Trade?> GetTradeByIdAsync(int id);

        /// <summary>
        /// Gets trade statistics for a user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>Trade statistics</returns>
        Task<TradeStatistics> GetTradeStatisticsAsync(string userId);
    }

    /// <summary>
    /// Trade statistics model
    /// </summary>
    public class TradeStatistics
    {
        public int TotalTrades { get; set; }
        public decimal TotalVolume { get; set; }
        public int BuyTrades { get; set; }
        public int SellTrades { get; set; }
        public decimal AverageTradeValue { get; set; }
    }
}