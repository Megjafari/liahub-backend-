using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LIAhub.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddManualApplicationFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsManual",
                table: "Applications",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Link",
                table: "Applications",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "Applications",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Applications",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Source",
                table: "Applications",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsManual",
                table: "Applications");

            migrationBuilder.DropColumn(
                name: "Link",
                table: "Applications");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "Applications");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Applications");

            migrationBuilder.DropColumn(
                name: "Source",
                table: "Applications");
        }
    }
}
