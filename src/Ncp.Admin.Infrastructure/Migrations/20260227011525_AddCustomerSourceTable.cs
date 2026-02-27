using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ncp.Admin.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerSourceTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomerSource",
                table: "customer");

            migrationBuilder.AddColumn<Guid>(
                name: "CustomerSourceId",
                table: "customer",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "CustomerSourceName",
                table: "customer",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "customer_source",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customer_source", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_customer_source_SortOrder",
                table: "customer_source",
                column: "SortOrder");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "customer_source");

            migrationBuilder.DropColumn(
                name: "CustomerSourceId",
                table: "customer");

            migrationBuilder.DropColumn(
                name: "CustomerSourceName",
                table: "customer");

            migrationBuilder.AddColumn<string>(
                name: "CustomerSource",
                table: "customer",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }
    }
}
