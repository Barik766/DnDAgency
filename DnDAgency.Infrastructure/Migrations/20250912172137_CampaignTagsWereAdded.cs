using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DnDAgency.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CampaignTagsWereAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CampaignTag_Campaigns_CampaignId",
                table: "CampaignTag");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CampaignTag",
                table: "CampaignTag");

            migrationBuilder.RenameTable(
                name: "CampaignTag",
                newName: "CampaignTags");

            migrationBuilder.RenameIndex(
                name: "IX_CampaignTag_CampaignId",
                table: "CampaignTags",
                newName: "IX_CampaignTags_CampaignId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CampaignTags",
                table: "CampaignTags",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CampaignTags_Campaigns_CampaignId",
                table: "CampaignTags",
                column: "CampaignId",
                principalTable: "Campaigns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CampaignTags_Campaigns_CampaignId",
                table: "CampaignTags");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CampaignTags",
                table: "CampaignTags");

            migrationBuilder.RenameTable(
                name: "CampaignTags",
                newName: "CampaignTag");

            migrationBuilder.RenameIndex(
                name: "IX_CampaignTags_CampaignId",
                table: "CampaignTag",
                newName: "IX_CampaignTag_CampaignId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CampaignTag",
                table: "CampaignTag",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CampaignTag_Campaigns_CampaignId",
                table: "CampaignTag",
                column: "CampaignId",
                principalTable: "Campaigns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
