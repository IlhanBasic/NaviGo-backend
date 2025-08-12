using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NaviGoApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemovedAllFieldsAboutPenaltyFromShipmentTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DelayHours",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "DelayPenaltyAmount",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "PenaltyCalculatedAt",
                table: "Shipments");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DelayHours",
                table: "Shipments",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DelayPenaltyAmount",
                table: "Shipments",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PenaltyCalculatedAt",
                table: "Shipments",
                type: "timestamp with time zone",
                nullable: true);
        }
    }
}
