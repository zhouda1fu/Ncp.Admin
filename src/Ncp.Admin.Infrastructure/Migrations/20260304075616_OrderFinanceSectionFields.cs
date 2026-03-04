using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ncp.Admin.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class OrderFinanceSectionFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ContractAmount",
                table: "order",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ContractDiscount",
                table: "order",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "ContractNotCompanyTemplate",
                table: "order",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsShipped",
                table: "order",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PaymentStatus",
                table: "order",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SelectedContractFileId",
                table: "order",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContractAmount",
                table: "order");

            migrationBuilder.DropColumn(
                name: "ContractDiscount",
                table: "order");

            migrationBuilder.DropColumn(
                name: "ContractNotCompanyTemplate",
                table: "order");

            migrationBuilder.DropColumn(
                name: "IsShipped",
                table: "order");

            migrationBuilder.DropColumn(
                name: "PaymentStatus",
                table: "order");

            migrationBuilder.DropColumn(
                name: "SelectedContractFileId",
                table: "order");
        }
    }
}
