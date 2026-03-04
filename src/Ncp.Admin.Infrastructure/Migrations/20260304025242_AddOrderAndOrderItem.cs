using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ncp.Admin.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderAndOrderItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "order",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: true),
                    OrderNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    Remark = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    OwnerId = table.Column<long>(type: "bigint", nullable: false),
                    OwnerName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ReceiverName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ReceiverPhone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ReceiverAddress = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    PayDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeliveryDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatorId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_order", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "order_item",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Model = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Qty = table.Column<int>(type: "integer", nullable: false),
                    Unit = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Price = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    Remark = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_order_item", x => x.Id);
                    table.ForeignKey(
                        name: "FK_order_item_order_OrderId",
                        column: x => x.OrderId,
                        principalTable: "order",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_order_CreatedAt",
                table: "order",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_order_CustomerId",
                table: "order",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_order_IsDeleted",
                table: "order",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_order_OrderNumber",
                table: "order",
                column: "OrderNumber");

            migrationBuilder.CreateIndex(
                name: "IX_order_OwnerId",
                table: "order",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_order_item_OrderId",
                table: "order_item",
                column: "OrderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "order_item");

            migrationBuilder.DropTable(
                name: "order");
        }
    }
}
