using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace api.Migrations
{
    /// <inheritdoc />
    public partial class AddConnections : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Point>(
                name: "Location",
                table: "Users",
                type: "geometry(Point, 4326)",
                nullable: false,
                oldClrType: typeof(Point),
                oldType: "geometry(Point, 4326)",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "ChatSessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    User1TelegramId = table.Column<long>(type: "bigint", nullable: false),
                    User2TelegramId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatSessions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Connections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FromTelegramId = table.Column<long>(type: "bigint", nullable: false),
                    ToTelegramId = table.Column<long>(type: "bigint", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Connections", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChatSessions_User1TelegramId_User2TelegramId",
                table: "ChatSessions",
                columns: new[] { "User1TelegramId", "User2TelegramId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Connections_FromTelegramId_ToTelegramId",
                table: "Connections",
                columns: new[] { "FromTelegramId", "ToTelegramId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChatSessions");

            migrationBuilder.DropTable(
                name: "Connections");

            migrationBuilder.AlterColumn<Point>(
                name: "Location",
                table: "Users",
                type: "geometry(Point, 4326)",
                nullable: true,
                oldClrType: typeof(Point),
                oldType: "geometry(Point, 4326)");
        }
    }
}
