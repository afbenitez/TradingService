using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using TradingService.Models;
using TradingService.Services.Interfaces;

namespace TradingService.Services
{
    /// <summary>
    /// RabbitMQ implementation of message queue service
    /// </summary>
    public class RabbitMQService : IMessageQueueService, IDisposable
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly ILogger<RabbitMQService> _logger;
        private readonly string _queueName;
        private readonly string _exchangeName;

        public RabbitMQService(ILogger<RabbitMQService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _queueName = configuration.GetValue<string>("RabbitMQ:Queue", "trades")!;
            _exchangeName = configuration.GetValue<string>("RabbitMQ:Exchange", "trading_exchange")!;
            
            var factory = new ConnectionFactory()
            {
                HostName = configuration.GetValue<string>("RabbitMQ:HostName", "localhost"),
                Port = configuration.GetValue<int>("RabbitMQ:Port", 5672),
                UserName = configuration.GetValue<string>("RabbitMQ:UserName", "guest"),
                Password = configuration.GetValue<string>("RabbitMQ:Password", "guest"),
                VirtualHost = configuration.GetValue<string>("RabbitMQ:VirtualHost", "/")
            };

            try
            {
                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();

                // Declare exchange and queue
                _channel.ExchangeDeclare(exchange: _exchangeName, type: ExchangeType.Direct, durable: true);
                _channel.QueueDeclare(queue: _queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
                _channel.QueueBind(queue: _queueName, exchange: _exchangeName, routingKey: "trade.executed");

                _logger.LogInformation("RabbitMQ connection established successfully. Exchange: {Exchange}, Queue: {Queue}", 
                    _exchangeName, _queueName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to establish RabbitMQ connection to {HostName}:{Port}", 
                    factory.HostName, factory.Port);
                throw;
            }
        }

        /// <summary>
        /// Publishes a trade message to RabbitMQ
        /// </summary>
        /// <param name="trade">The trade to publish</param>
        public async Task PublishTradeAsync(Trade trade)
        {
            try
            {
                var message = new
                {
                    trade.Id,
                    trade.Symbol,
                    trade.Quantity,
                    trade.Price,
                    TradeType = trade.TradeType.ToString(),
                    trade.ExecutedAt,
                    trade.UserId,
                    Status = trade.Status.ToString(),
                    trade.TotalValue,
                    PublishedAt = DateTime.UtcNow
                };

                var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message, new JsonSerializerOptions 
                { 
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
                }));

                var properties = _channel.CreateBasicProperties();
                properties.Persistent = true;
                properties.MessageId = Guid.NewGuid().ToString();
                properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
                properties.ContentType = "application/json";

                _channel.BasicPublish(
                    exchange: _exchangeName,
                    routingKey: "trade.executed",
                    basicProperties: properties,
                    body: body);

                _logger.LogInformation("Trade message published successfully. TradeId: {TradeId}, Symbol: {Symbol}, Exchange: {Exchange}", 
                    trade.Id, trade.Symbol, _exchangeName);

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish trade message. TradeId: {TradeId}", trade.Id);
                throw;
            }
        }

        public void Dispose()
        {
            try
            {
                _channel?.Close();
                _channel?.Dispose();
                _connection?.Close();
                _connection?.Dispose();
                _logger.LogInformation("RabbitMQ connection closed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error closing RabbitMQ connection");
            }
        }
    }
}