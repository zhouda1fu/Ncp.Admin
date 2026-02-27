using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ncp.Admin.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerRegionNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CityName",
                table: "customer",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DistrictName",
                table: "customer",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProvinceName",
                table: "customer",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CityName",
                table: "customer");

            migrationBuilder.DropColumn(
                name: "DistrictName",
                table: "customer");

            migrationBuilder.DropColumn(
                name: "ProvinceName",
                table: "customer");
        }
    }
}
