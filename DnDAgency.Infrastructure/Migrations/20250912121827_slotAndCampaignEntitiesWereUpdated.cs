using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DnDAgency.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class slotAndCampaignEntitiesWereUpdated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxPlayers",
                table: "Slots");

            migrationBuilder.AddColumn<int>(
                name: "DurationHours",
                table: "Campaigns",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Campaigns",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Level",
                table: "Campaigns",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MaxPlayers",
                table: "Campaigns",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "CampaignTag",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    CampaignId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignTag", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CampaignTag_Campaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "Campaigns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CampaignTag_CampaignId",
                table: "CampaignTag",
                column: "CampaignId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CampaignTag");

            migrationBuilder.DropColumn(
                name: "DurationHours",
                table: "Campaigns");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Campaigns");

            migrationBuilder.DropColumn(
                name: "Level",
                table: "Campaigns");

            migrationBuilder.DropColumn(
                name: "MaxPlayers",
                table: "Campaigns");

            migrationBuilder.AddColumn<int>(
                name: "MaxPlayers",
                table: "Slots",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
