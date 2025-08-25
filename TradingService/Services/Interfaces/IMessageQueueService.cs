using TradingService.Models;

namespace TradingService.Services.Interfaces
{
    /// <summary>
    /// Interface for message queue service
    /// </summary>
    public interface IMessageQueueService
    {
        /// <summary>
        /// Publishes a trade message to the queue
        /// </summary>
        /// <param name="trade">The trade to publish</param>
        Task PublishTradeAsync(Trade trade);
    }
}