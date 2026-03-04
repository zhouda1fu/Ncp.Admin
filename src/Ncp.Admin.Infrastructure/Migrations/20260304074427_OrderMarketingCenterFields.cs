using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ncp.Admin.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class OrderMarketingCenterFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ContractSigningCompany",
                table: "order",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ContractTrustee",
                table: "order",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "DeptId",
                table: "order",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "DeptName",
                table: "order",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "EstimatedFreight",
                table: "order",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "InstallationFee",
                table: "order",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "NeedInvoice",
                table: "order",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ProjectContactName",
                table: "order",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ProjectContactPhone",
                table: "order",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Warranty",
                table: "order",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_order_DeptId",
                table: "order",
                column: "DeptId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_order_DeptId",
                table: "order");

            migrationBuilder.DropColumn(
                name: "ContractSigningCompany",
                table: "order");

            migrationBuilder.DropColumn(
                name: "ContractTrustee",
                table: "order");

            migrationBuilder.DropColumn(
                name: "DeptId",
                table: "order");

            migrationBuilder.DropColumn(
                name: "DeptName",
                table: "order");

            migrationBuilder.DropColumn(
                name: "EstimatedFreight",
                table: "order");

            migrationBuilder.DropColumn(
                name: "InstallationFee",
                table: "order");

            migrationBuilder.DropColumn(
                name: "NeedInvoice",
                table: "order");

            migrationBuilder.DropColumn(
                name: "ProjectContactName",
                table: "order");

            migrationBuilder.DropColumn(
                name: "ProjectContactPhone",
                table: "order");

            migrationBuilder.DropColumn(
                name: "Warranty",
                table: "order");
        }
    }
}
