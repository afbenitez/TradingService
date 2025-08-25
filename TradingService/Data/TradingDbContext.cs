using Microsoft.EntityFrameworkCore;
using TradingService.Models;

namespace TradingService.Data
{
    /// <summary>
    /// Database context for the trading service
    /// </summary>
    public class TradingDbContext : DbContext
    {
        public TradingDbContext(DbContextOptions<TradingDbContext> options) : base(options)
        {
        }

        /// <summary>
        /// DbSet for trades
        /// </summary>
        public DbSet<Trade> Trades { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Trade entity
            modelBuilder.Entity<Trade>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                // Configure table name in lowercase for PostgreSQL convention
                entity.ToTable("trades");
                
                // Configure properties
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Symbol).IsRequired().HasMaxLength(10).HasColumnName("symbol");
                entity.Property(e => e.UserId).IsRequired().HasMaxLength(50).HasColumnName("user_id");
                entity.Property(e => e.Quantity).HasColumnName("quantity");
                entity.Property(e => e.Price).HasColumnType("decimal(18,2)").HasColumnName("price");
                entity.Property(e => e.TradeType).HasColumnName("trade_type");
                entity.Property(e => e.Status).HasColumnName("status");
                entity.Property(e => e.ExecutedAt)
                    .HasColumnName("executed_at")
                    .HasDefaultValueSql("NOW()"); // PostgreSQL function for current timestamp
                
                // Index for performance
                entity.HasIndex(e => e.UserId).HasDatabaseName("ix_trades_user_id");
                entity.HasIndex(e => e.Symbol).HasDatabaseName("ix_trades_symbol");
                entity.HasIndex(e => e.ExecutedAt).HasDatabaseName("ix_trades_executed_at");
            });

            // Seed data for testing
            modelBuilder.Entity<Trade>().HasData(
                new Trade
                {
                    Id = 1,
                    Symbol = "AAPL",
                    Quantity = 100,
                    Price = 150.50m,
                    TradeType = TradeType.Buy,
                    ExecutedAt = DateTime.UtcNow.AddDays(-1),
                    UserId = "user1",
                    Status = TradeStatus.Executed
                },
                new Trade
                {
                    Id = 2,
                    Symbol = "GOOGL",
                    Quantity = 50,
                    Price = 2800.75m,
                    TradeType = TradeType.Sell,
                    ExecutedAt = DateTime.UtcNow.AddHours(-5),
                    UserId = "user2",
                    Status = TradeStatus.Executed
                }
            );
        }
    }
}