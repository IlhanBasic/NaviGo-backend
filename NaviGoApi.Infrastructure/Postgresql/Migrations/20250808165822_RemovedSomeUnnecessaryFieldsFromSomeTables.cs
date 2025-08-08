using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NaviGoApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemovedSomeUnnecessaryFieldsFromSomeTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAvailable",
                table: "Vehicles");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsAvailable",
                table: "Vehicles",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
