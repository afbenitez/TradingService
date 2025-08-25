using TradingService.Models;
using TradingService.Services.Interfaces;

namespace TradingService.Services
{
    /// <summary>
    /// Dummy implementation of message queue service for when RabbitMQ is not available
    /// </summary>
    public class DummyMessageQueueService : IMessageQueueService
    {
        private readonly ILogger<DummyMessageQueueService> _logger;

        public DummyMessageQueueService(ILogger<DummyMessageQueueService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Simulates publishing a trade message (logs only)
        /// </summary>
        /// <param name="trade">The trade to publish</param>
        public Task PublishTradeAsync(Trade trade)
        {
            _logger.LogWarning("?? RabbitMQ not available - trade message would be published: TradeId={TradeId}, Symbol={Symbol}, User={UserId}, Type={TradeType}, Amount={TotalValue}", 
                trade.Id, trade.Symbol, trade.UserId, trade.TradeType, trade.TotalValue);
            
            return Task.CompletedTask;
        }
    }
}