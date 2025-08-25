using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace TradingConsumer.Services
{
    /// <summary>
    /// Service for consuming trade messages from RabbitMQ
    /// </summary>
    public class TradeMessageConsumer : IDisposable
    {
        private readonly ILogger<TradeMessageConsumer> _logger;
        private readonly IConfiguration _configuration;
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly string _queueName;
        private readonly string _exchangeName;

        public TradeMessageConsumer(ILogger<TradeMessageConsumer> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
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

                // Ensure queue exists
                _channel.QueueDeclare(queue: _queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);

                _logger.LogInformation("Connected to RabbitMQ. Queue: {Queue}", _queueName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to connect to RabbitMQ");
                throw;
            }
        }

        /// <summary>
        /// Starts consuming messages from the queue
        /// </summary>
        public void StartConsuming()
        {
            var consumer = new EventingBasicConsumer(_channel);
            
            consumer.Received += (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    
                    _logger.LogInformation("Received trade message: {Message}", message);

                    // Parse the trade message
                    var tradeData = JsonSerializer.Deserialize<TradeMessage>(message, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (tradeData != null)
                    {
                        LogTradeDetails(tradeData);
                    }

                    // Acknowledge the message
                    _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing trade message");
                    
                    // Reject the message and requeue it
                    _channel.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, requeue: true);
                }
            };

            _channel.BasicConsume(queue: _queueName, autoAck: false, consumer: consumer);
            
            _logger.LogInformation("Started consuming messages from queue: {Queue}", _queueName);
        }

        /// <summary>
        /// Logs detailed trade information
        /// </summary>
        private void LogTradeDetails(TradeMessage trade)
        {
            _logger.LogInformation("""
                ===== TRADE EXECUTED =====
                Trade ID: {TradeId}
                Symbol: {Symbol}
                Quantity: {Quantity}
                Price: ${Price:F2}
                Trade Type: {TradeType}
                Total Value: ${TotalValue:F2}
                User ID: {UserId}
                Status: {Status}
                Executed At: {ExecutedAt}
                Published At: {PublishedAt}
                ===========================
                """,
                trade.Id,
                trade.Symbol,
                trade.Quantity,
                trade.Price,
                trade.TradeType,
                trade.TotalValue,
                trade.UserId,
                trade.Status,
                trade.ExecutedAt,
                trade.PublishedAt);
        }

        public void Dispose()
        {
            try
            {
                _channel?.Close();
                _channel?.Dispose();
                _connection?.Close();
                _connection?.Dispose();
                _logger.LogInformation("RabbitMQ connection closed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error closing RabbitMQ connection");
            }
        }
    }

    /// <summary>
    /// Model for trade messages
    /// </summary>
    public class TradeMessage
    {
        public int Id { get; set; }
        public string Symbol { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public string TradeType { get; set; } = string.Empty;
        public DateTime ExecutedAt { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal TotalValue { get; set; }
        public DateTime PublishedAt { get; set; }
    }
}