using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DnDAgency.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RoomEntityWasAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1) Создать таблицу Room
            migrationBuilder.CreateTable(
                name: "Room",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Capacity = table.Column<int>(type: "integer", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Room", x => x.Id);
                });

            // 2) Вставить дефолтную комнату
            migrationBuilder.Sql(@"
                INSERT INTO ""Room"" (""Id"", ""Name"", ""Type"", ""IsActive"", ""CreatedAt"")
                VALUES ('00000000-0000-0000-0000-000000000000', 'Default Room', 0, true, NOW())
                ON CONFLICT (""Id"") DO NOTHING;
            ");

            // 3) Добавить колонку RoomId в Campaigns с default = дефолтная комната
            migrationBuilder.AddColumn<Guid>(
                name: "RoomId",
                table: "Campaigns",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            // 4) Добавить колонку PlayersCount в Bookings
            migrationBuilder.AddColumn<int>(
                name: "PlayersCount",
                table: "Bookings",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            // 5) Создать индекс на RoomId
            migrationBuilder.CreateIndex(
                name: "IX_Campaigns_RoomId",
                table: "Campaigns",
                column: "RoomId");

            // 6) Добавить внешний ключ
            migrationBuilder.AddForeignKey(
                name: "FK_Campaigns_Room_RoomId",
                table: "Campaigns",
                column: "RoomId",
                principalTable: "Room",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Убрать FK
            migrationBuilder.DropForeignKey(
                name: "FK_Campaigns_Room_RoomId",
                table: "Campaigns");

            // Удалить индекс
            migrationBuilder.DropIndex(
                name: "IX_Campaigns_RoomId",
                table: "Campaigns");

            // Удалить колонку RoomId из Campaigns
            migrationBuilder.DropColumn(
                name: "RoomId",
                table: "Campaigns");

            // Удалить PlayersCount из Bookings
            migrationBuilder.DropColumn(
                name: "PlayersCount",
                table: "Bookings");

            // Удалить дефолтную комнату
            migrationBuilder.Sql(@"
                DELETE FROM ""Room"" WHERE ""Id"" = '00000000-0000-0000-0000-000000000000';
            ");

            // Удалить таблицу Room
            migrationBuilder.DropTable(
                name: "Room");
        }
    }
}
