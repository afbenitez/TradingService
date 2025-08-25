using System.ComponentModel.DataAnnotations;

namespace TradingService.Models
{
    /// <summary>
    /// Represents a trade entity in the trading system
    /// </summary>
    public class Trade
    {
        /// <summary>
        /// Unique identifier for the trade
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Symbol of the traded asset (e.g., AAPL, GOOGL)
        /// </summary>
        [Required]
        [StringLength(10)]
        public string Symbol { get; set; } = string.Empty;

        /// <summary>
        /// Number of shares traded
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be positive")]
        public int Quantity { get; set; }

        /// <summary>
        /// Price per share at the time of trade
        /// </summary>
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be positive")]
        public decimal Price { get; set; }

        /// <summary>
        /// Type of trade: BUY or SELL
        /// </summary>
        [Required]
        public TradeType TradeType { get; set; }

        /// <summary>
        /// Timestamp when the trade was executed
        /// </summary>
        public DateTime ExecutedAt { get; set; }

        /// <summary>
        /// User ID who executed the trade
        /// </summary>
        [Required]
        [StringLength(50)]
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// Current status of the trade
        /// </summary>
        public TradeStatus Status { get; set; }

        /// <summary>
        /// Total value of the trade (Quantity * Price)
        /// </summary>
        public decimal TotalValue => Quantity * Price;
    }

    /// <summary>
    /// Enumeration for trade types
    /// </summary>
    public enum TradeType
    {
        Buy = 1,
        Sell = 2
    }

    /// <summary>
    /// Enumeration for trade status
    /// </summary>
    public enum TradeStatus
    {
        Pending = 1,
        Executed = 2,
        Failed = 3,
        Cancelled = 4
    }
}