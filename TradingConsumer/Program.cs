using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using TradingConsumer.Services;

namespace TradingConsumer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Configure Serilog
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
                .WriteTo.File("logs/consumer-.txt", rollingInterval: RollingInterval.Day)
                .Enrich.FromLogContext()
                .CreateLogger();

            try
            {
                Log.Information("Starting Trading Consumer Application");

                var host = CreateHostBuilder(args).Build();
                
                // Start the consumer service
                var consumerService = host.Services.GetRequiredService<TradeMessageConsumer>();
                consumerService.StartConsuming();

                Log.Information("Trading Consumer started successfully. Press Ctrl+C to exit.");

                // Keep the application running
                await host.RunAsync();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Trading Consumer terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .ConfigureServices((context, services) =>
                {
                    services.AddSingleton<TradeMessageConsumer>();
                });
    }
}