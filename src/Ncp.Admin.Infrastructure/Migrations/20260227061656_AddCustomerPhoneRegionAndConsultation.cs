using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ncp.Admin.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerPhoneRegionAndConsultation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StatusId",
                table: "customer");

            migrationBuilder.AddColumn<string>(
                name: "ConsultationContent",
                table: "customer",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContactQq",
                table: "customer",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContactWechat",
                table: "customer",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsVoided",
                table: "customer",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PhoneCityCode",
                table: "customer",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhoneCityName",
                table: "customer",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhoneDistrictCode",
                table: "customer",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhoneDistrictName",
                table: "customer",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhoneProvinceCode",
                table: "customer",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhoneProvinceName",
                table: "customer",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConsultationContent",
                table: "customer");

            migrationBuilder.DropColumn(
                name: "ContactQq",
                table: "customer");

            migrationBuilder.DropColumn(
                name: "ContactWechat",
                table: "customer");

            migrationBuilder.DropColumn(
                name: "IsVoided",
                table: "customer");

            migrationBuilder.DropColumn(
                name: "PhoneCityCode",
                table: "customer");

            migrationBuilder.DropColumn(
                name: "PhoneCityName",
                table: "customer");

            migrationBuilder.DropColumn(
                name: "PhoneDistrictCode",
                table: "customer");

            migrationBuilder.DropColumn(
                name: "PhoneDistrictName",
                table: "customer");

            migrationBuilder.DropColumn(
                name: "PhoneProvinceCode",
                table: "customer");

            migrationBuilder.DropColumn(
                name: "PhoneProvinceName",
                table: "customer");

            migrationBuilder.AddColumn<int>(
                name: "StatusId",
                table: "customer",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
