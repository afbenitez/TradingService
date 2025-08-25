using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TradingService.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "trades",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    symbol = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    price = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    trade_type = table.Column<int>(type: "integer", nullable: false),
                    executed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    user_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trades", x => x.id);
                });

            migrationBuilder.InsertData(
                table: "trades",
                columns: new[] { "id", "executed_at", "price", "quantity", "status", "symbol", "trade_type", "user_id" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 8, 23, 17, 29, 10, 698, DateTimeKind.Utc).AddTicks(8066), 150.50m, 100, 2, "AAPL", 1, "user1" },
                    { 2, new DateTime(2025, 8, 24, 12, 29, 10, 698, DateTimeKind.Utc).AddTicks(8083), 2800.75m, 50, 2, "GOOGL", 2, "user2" }
                });

            migrationBuilder.CreateIndex(
                name: "ix_trades_executed_at",
                table: "trades",
                column: "executed_at");

            migrationBuilder.CreateIndex(
                name: "ix_trades_symbol",
                table: "trades",
                column: "symbol");

            migrationBuilder.CreateIndex(
                name: "ix_trades_user_id",
                table: "trades",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "trades");
        }
    }
}
