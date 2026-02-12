using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ncp.Admin.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRoleDataScope : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ApprovalMode",
                table: "workflow_node",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<long>(
                name: "InitiatorDeptId",
                table: "workflow_instance",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<int>(
                name: "DataScope",
                table: "role",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "notification",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    Title = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Content = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Level = table.Column<int>(type: "int", nullable: false),
                    SenderId = table.Column<long>(type: "bigint", nullable: true),
                    SenderName = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ReceiverId = table.Column<long>(type: "bigint", nullable: false),
                    IsRead = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ReadAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true),
                    BusinessId = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BusinessType = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notification", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "position",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Code = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DeptId = table.Column<long>(type: "bigint", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    UpdateTime = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_position", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_workflow_instance_InitiatorDeptId",
                table: "workflow_instance",
                column: "InitiatorDeptId");

            migrationBuilder.CreateIndex(
                name: "IX_notification_IsDeleted",
                table: "notification",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_notification_IsRead",
                table: "notification",
                column: "IsRead");

            migrationBuilder.CreateIndex(
                name: "IX_notification_ReceiverId",
                table: "notification",
                column: "ReceiverId");

            migrationBuilder.CreateIndex(
                name: "IX_notification_ReceiverId_IsRead_IsDeleted",
                table: "notification",
                columns: new[] { "ReceiverId", "IsRead", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_notification_Type",
                table: "notification",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_position_Code",
                table: "position",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_position_DeptId",
                table: "position",
                column: "DeptId");

            migrationBuilder.CreateIndex(
                name: "IX_position_IsDeleted",
                table: "position",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_position_Status",
                table: "position",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "notification");

            migrationBuilder.DropTable(
                name: "position");

            migrationBuilder.DropIndex(
                name: "IX_workflow_instance_InitiatorDeptId",
                table: "workflow_instance");

            migrationBuilder.DropColumn(
                name: "ApprovalMode",
                table: "workflow_node");

            migrationBuilder.DropColumn(
                name: "InitiatorDeptId",
                table: "workflow_instance");

            migrationBuilder.DropColumn(
                name: "DataScope",
                table: "role");
        }
    }
}
