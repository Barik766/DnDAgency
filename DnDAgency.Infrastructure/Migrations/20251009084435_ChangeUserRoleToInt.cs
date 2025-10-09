using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DnDAgency.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ChangeUserRoleToInt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Привести все значения к числам (если вдруг остались строки)
            migrationBuilder.Sql("UPDATE \"Users\" SET \"Role\" = '2' WHERE \"Role\" = 'Admin';");
            migrationBuilder.Sql("UPDATE \"Users\" SET \"Role\" = '1' WHERE \"Role\" = 'Master';");
            migrationBuilder.Sql("UPDATE \"Users\" SET \"Role\" = '0' WHERE \"Role\" = 'Player';");

            // Изменить тип столбца с помощью USING ...::integer
            migrationBuilder.Sql("ALTER TABLE \"Users\" ALTER COLUMN \"Role\" TYPE integer USING \"Role\"::integer;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Вернуть обратно в строку (enum name)
            migrationBuilder.Sql("UPDATE \"Users\" SET \"Role\" = 'Admin' WHERE \"Role\" = 2;");
            migrationBuilder.Sql("UPDATE \"Users\" SET \"Role\" = 'Master' WHERE \"Role\" = 1;");
            migrationBuilder.Sql("UPDATE \"Users\" SET \"Role\" = 'Player' WHERE \"Role\" = 0;");

            migrationBuilder.AlterColumn<string>(
                name: "Role",
                table: "Users",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");
        }
    }
}