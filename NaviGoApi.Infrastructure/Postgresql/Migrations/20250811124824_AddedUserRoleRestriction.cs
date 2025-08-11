using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NaviGoApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddedUserRoleRestriction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_User_CompanyId_Role",
                table: "Users");

            migrationBuilder.AddCheckConstraint(
                name: "CK_User_CompanyId_Role",
                table: "Users",
                sql: "\r\n					(\"UserRole\" = 4 AND \"CompanyId\" IS NULL) OR\r\n					(\"UserRole\" = 1) OR\r\n					(\"UserRole\" IN (2, 3) AND \"CompanyId\" IS NOT NULL)\r\n				");

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_ContractNumber",
                table: "Contracts",
                column: "ContractNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_User_CompanyId_Role",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Contracts_ContractNumber",
                table: "Contracts");

            migrationBuilder.AddCheckConstraint(
                name: "CK_User_CompanyId_Role",
                table: "Users",
                sql: "\r\n					(\"UserRole\" = 1 AND \"CompanyId\" IS NULL) OR\r\n					(\"UserRole\" IN (2, 3) AND \"CompanyId\" IS NOT NULL) OR\r\n					(\"UserRole\" = 4 AND \"CompanyId\" IS NULL)\r\n				");
        }
    }
}
