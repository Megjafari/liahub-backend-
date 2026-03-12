using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LIAhub.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkMode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "WorkMode",
                table: "CachedJobs",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WorkMode",
                table: "CachedJobs");
        }
    }
}
