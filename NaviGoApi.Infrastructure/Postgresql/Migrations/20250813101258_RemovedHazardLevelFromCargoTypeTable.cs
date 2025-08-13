using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NaviGoApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemovedHazardLevelFromCargoTypeTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HazardLevel",
                table: "CargoTypes");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "HazardLevel",
                table: "CargoTypes",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
