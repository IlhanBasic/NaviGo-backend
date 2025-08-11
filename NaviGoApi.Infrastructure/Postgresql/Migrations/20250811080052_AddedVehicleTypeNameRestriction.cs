using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NaviGoApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddedVehicleTypeNameRestriction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_VehicleTypes_TypeName",
                table: "VehicleTypes",
                column: "TypeName",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_VehicleTypes_TypeName",
                table: "VehicleTypes");
        }
    }
}
