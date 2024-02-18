using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StatisticsBot.Services.Data.Migrations
{
    /// <inheritdoc />
    public partial class CreateTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ChartMessages",
                columns: table => new
                {
                    ChatId = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MessageId = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChartMessages", x => x.ChatId);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    TelegramId = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CodewarsLogin = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Rank = table.Column<int>(type: "INTEGER", nullable: true),
                    Honor = table.Column<int>(type: "INTEGER", nullable: true),
                    TotalTasks = table.Column<int>(type: "INTEGER", nullable: true),
                    LastUpdate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ChatId = table.Column<long>(type: "INTEGER", nullable: false),
                    ColorHex = table.Column<string>(type: "TEXT", maxLength: 7, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.TelegramId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChartMessages");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
