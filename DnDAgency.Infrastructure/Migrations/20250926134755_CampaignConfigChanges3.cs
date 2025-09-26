using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DnDAgency.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CampaignConfigChanges3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CampaignRoom_Campaigns_CampaignId",
                table: "CampaignRoom");

            migrationBuilder.DropForeignKey(
                name: "FK_CampaignRoom_Rooms_RoomId",
                table: "CampaignRoom");

            migrationBuilder.RenameColumn(
                name: "RoomId",
                table: "CampaignRoom",
                newName: "RoomsId");

            migrationBuilder.RenameColumn(
                name: "CampaignId",
                table: "CampaignRoom",
                newName: "CampaignsId");

            migrationBuilder.RenameIndex(
                name: "IX_CampaignRoom_RoomId",
                table: "CampaignRoom",
                newName: "IX_CampaignRoom_RoomsId");

            migrationBuilder.AddForeignKey(
                name: "FK_CampaignRoom_Campaigns_CampaignsId",
                table: "CampaignRoom",
                column: "CampaignsId",
                principalTable: "Campaigns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CampaignRoom_Rooms_RoomsId",
                table: "CampaignRoom",
                column: "RoomsId",
                principalTable: "Rooms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CampaignRoom_Campaigns_CampaignsId",
                table: "CampaignRoom");

            migrationBuilder.DropForeignKey(
                name: "FK_CampaignRoom_Rooms_RoomsId",
                table: "CampaignRoom");

            migrationBuilder.RenameColumn(
                name: "RoomsId",
                table: "CampaignRoom",
                newName: "RoomId");

            migrationBuilder.RenameColumn(
                name: "CampaignsId",
                table: "CampaignRoom",
                newName: "CampaignId");

            migrationBuilder.RenameIndex(
                name: "IX_CampaignRoom_RoomsId",
                table: "CampaignRoom",
                newName: "IX_CampaignRoom_RoomId");

            migrationBuilder.AddForeignKey(
                name: "FK_CampaignRoom_Campaigns_CampaignId",
                table: "CampaignRoom",
                column: "CampaignId",
                principalTable: "Campaigns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CampaignRoom_Rooms_RoomId",
                table: "CampaignRoom",
                column: "RoomId",
                principalTable: "Rooms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
