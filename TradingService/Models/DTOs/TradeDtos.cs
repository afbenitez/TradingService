using System.ComponentModel.DataAnnotations;

namespace TradingService.Models.DTOs
{
    /// <summary>
    /// Data Transfer Object for creating a new trade
    /// </summary>
    public class CreateTradeDto
    {
        /// <summary>
        /// Symbol of the asset to trade
        /// </summary>
        [Required(ErrorMessage = "Symbol is required")]
        [StringLength(10, ErrorMessage = "Symbol cannot exceed 10 characters")]
        public string Symbol { get; set; } = string.Empty;

        /// <summary>
        /// Quantity of shares to trade
        /// </summary>
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be positive")]
        public int Quantity { get; set; }

        /// <summary>
        /// Price per share
        /// </summary>
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be positive")]
        public decimal Price { get; set; }

        /// <summary>
        /// Type of trade (Buy = 1, Sell = 2)
        /// </summary>
        [Required]
        [Range(1, 2, ErrorMessage = "TradeType must be 1 (Buy) or 2 (Sell)")]
        public TradeType TradeType { get; set; }

        /// <summary>
        /// User ID executing the trade
        /// </summary>
        [Required(ErrorMessage = "UserId is required")]
        [StringLength(50, ErrorMessage = "UserId cannot exceed 50 characters")]
        public string UserId { get; set; } = string.Empty;
    }

    /// <summary>
    /// Data Transfer Object for trade responses
    /// </summary>
    public class TradeDto
    {
        /// <summary>
        /// Trade identifier
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Asset symbol
        /// </summary>
        public string Symbol { get; set; } = string.Empty;

        /// <summary>
        /// Quantity traded
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// Price per share
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// Trade type
        /// </summary>
        public string TradeType { get; set; } = string.Empty;

        /// <summary>
        /// Execution timestamp
        /// </summary>
        public DateTime ExecutedAt { get; set; }

        /// <summary>
        /// User who executed the trade
        /// </summary>
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// Trade status
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Total value of the trade
        /// </summary>
        public decimal TotalValue { get; set; }
    }

    /// <summary>
    /// Query parameters for filtering trades
    /// </summary>
    public class TradeQueryDto
    {
        /// <summary>
        /// Filter by user ID
        /// </summary>
        public string? UserId { get; set; }

        /// <summary>
        /// Filter by symbol
        /// </summary>
        public string? Symbol { get; set; }

        /// <summary>
        /// Filter by trade type
        /// </summary>
        public TradeType? TradeType { get; set; }

        /// <summary>
        /// Filter trades from this date
        /// </summary>
        public DateTime? FromDate { get; set; }

        /// <summary>
        /// Filter trades to this date
        /// </summary>
        public DateTime? ToDate { get; set; }

        /// <summary>
        /// Page number for pagination
        /// </summary>
        [Range(1, int.MaxValue)]
        public int Page { get; set; } = 1;

        /// <summary>
        /// Page size for pagination
        /// </summary>
        [Range(1, 100)]
        public int PageSize { get; set; } = 10;
    }
}