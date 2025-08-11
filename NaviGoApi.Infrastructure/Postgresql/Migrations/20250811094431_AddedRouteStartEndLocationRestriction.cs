using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NaviGoApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddedRouteStartEndLocationRestriction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddCheckConstraint(
                name: "CK_Route_StartEndLocation_Different",
                table: "Routes",
                sql: "\"StartLocationId\" <> \"EndLocationId\"");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Companies_MaxCommissionRate_OnlyForwarder",
                table: "Companies",
                sql: "(\"MaxCommissionRate\" IS NULL OR \"CompanyType\" = 2)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Route_StartEndLocation_Different",
                table: "Routes");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Companies_MaxCommissionRate_OnlyForwarder",
                table: "Companies");
        }
    }
}
