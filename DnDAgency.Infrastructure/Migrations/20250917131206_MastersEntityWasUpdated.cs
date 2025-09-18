using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DnDAgency.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MastersEntityWasUpdated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PhotoUrl",
                table: "Masters",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PhotoUrl",
                table: "Masters");
        }
    }
}
