using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LIAhub.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddJobScoringFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<List<string>>(
                name: "NegativeSignals",
                table: "CachedJobs",
                type: "text[]",
                nullable: false);

            migrationBuilder.AddColumn<int>(
                name: "RelevanceScore",
                table: "CachedJobs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<List<string>>(
                name: "StudentSignals",
                table: "CachedJobs",
                type: "text[]",
                nullable: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NegativeSignals",
                table: "CachedJobs");

            migrationBuilder.DropColumn(
                name: "RelevanceScore",
                table: "CachedJobs");

            migrationBuilder.DropColumn(
                name: "StudentSignals",
                table: "CachedJobs");
        }
    }
}
