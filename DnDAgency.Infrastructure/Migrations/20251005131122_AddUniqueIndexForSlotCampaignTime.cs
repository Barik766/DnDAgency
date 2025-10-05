using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DnDAgency.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueIndexForSlotCampaignTime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Slots_CampaignId",
                table: "Slots");

            migrationBuilder.CreateIndex(
                name: "IX_Slots_CampaignId_StartTime",
                table: "Slots",
                columns: new[] { "CampaignId", "StartTime" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Slots_CampaignId_StartTime",
                table: "Slots");

            migrationBuilder.CreateIndex(
                name: "IX_Slots_CampaignId",
                table: "Slots",
                column: "CampaignId");
        }
    }
}
