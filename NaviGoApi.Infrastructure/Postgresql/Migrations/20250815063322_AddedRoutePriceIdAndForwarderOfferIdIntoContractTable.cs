using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NaviGoApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddedRoutePriceIdAndForwarderOfferIdIntoContractTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ForwarderOfferId",
                table: "Contracts",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RoutePriceId",
                table: "Contracts",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_ForwarderOfferId",
                table: "Contracts",
                column: "ForwarderOfferId");

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_RoutePriceId",
                table: "Contracts",
                column: "RoutePriceId");

            migrationBuilder.AddForeignKey(
                name: "FK_Contracts_ForwarderOffers_ForwarderOfferId",
                table: "Contracts",
                column: "ForwarderOfferId",
                principalTable: "ForwarderOffers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Contracts_RoutesPrices_RoutePriceId",
                table: "Contracts",
                column: "RoutePriceId",
                principalTable: "RoutesPrices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Contracts_ForwarderOffers_ForwarderOfferId",
                table: "Contracts");

            migrationBuilder.DropForeignKey(
                name: "FK_Contracts_RoutesPrices_RoutePriceId",
                table: "Contracts");

            migrationBuilder.DropIndex(
                name: "IX_Contracts_ForwarderOfferId",
                table: "Contracts");

            migrationBuilder.DropIndex(
                name: "IX_Contracts_RoutePriceId",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "ForwarderOfferId",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "RoutePriceId",
                table: "Contracts");
        }
    }
}
