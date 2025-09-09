using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NaviGoApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MakeNullableDriverIdVehicleIdCorrectThisTime : Migration
    {
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AlterColumn<int>(
				name: "VehicleId",
				table: "Shipments",
				type: "integer",
				nullable: true,
				oldClrType: typeof(int),
				oldType: "integer");

			migrationBuilder.AlterColumn<int>(
				name: "DriverId",
				table: "Shipments",
				type: "integer",
				nullable: true,
				oldClrType: typeof(int),
				oldType: "integer");
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AlterColumn<int>(
				name: "VehicleId",
				table: "Shipments",
				type: "integer",
				nullable: false,
				defaultValue: 0,
				oldClrType: typeof(int),
				oldType: "integer",
				oldNullable: true);

			migrationBuilder.AlterColumn<int>(
				name: "DriverId",
				table: "Shipments",
				type: "integer",
				nullable: false,
				defaultValue: 0,
				oldClrType: typeof(int),
				oldType: "integer",
				oldNullable: true);
		}

	}
}
