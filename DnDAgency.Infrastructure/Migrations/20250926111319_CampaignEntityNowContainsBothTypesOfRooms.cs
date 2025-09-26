using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DnDAgency.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CampaignEntityNowContainsBothTypesOfRooms : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Campaigns_Rooms_RoomId",
                table: "Campaigns");

            migrationBuilder.DropIndex(
                name: "IX_Campaigns_RoomId",
                table: "Campaigns");

            migrationBuilder.DropColumn(
                name: "RoomId",
                table: "Campaigns");

            migrationBuilder.CreateTable(
                name: "CampaignRoom",
                columns: table => new
                {
                    CampaignsId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoomsId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignRoom", x => new { x.CampaignsId, x.RoomsId });
                    table.ForeignKey(
                        name: "FK_CampaignRoom_Campaigns_CampaignsId",
                        column: x => x.CampaignsId,
                        principalTable: "Campaigns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CampaignRoom_Rooms_RoomsId",
                        column: x => x.RoomsId,
                        principalTable: "Rooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CampaignRoom_RoomsId",
                table: "CampaignRoom",
                column: "RoomsId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CampaignRoom");

            migrationBuilder.AddColumn<Guid>(
                name: "RoomId",
                table: "Campaigns",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Campaigns_RoomId",
                table: "Campaigns",
                column: "RoomId");

            migrationBuilder.AddForeignKey(
                name: "FK_Campaigns_Rooms_RoomId",
                table: "Campaigns",
                column: "RoomId",
                principalTable: "Rooms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
