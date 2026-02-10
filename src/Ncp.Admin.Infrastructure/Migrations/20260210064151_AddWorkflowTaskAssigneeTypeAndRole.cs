using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ncp.Admin.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkflowTaskAssigneeTypeAndRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "AssigneeId",
                table: "workflow_task",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<Guid>(
                name: "AssigneeRoleId",
                table: "workflow_task",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<int>(
                name: "AssigneeType",
                table: "workflow_task",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Version",
                table: "workflow_task",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "FailureReason",
                table: "workflow_instance",
                type: "varchar(2000)",
                maxLength: 2000,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_workflow_task_AssigneeRoleId",
                table: "workflow_task",
                column: "AssigneeRoleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_workflow_task_AssigneeRoleId",
                table: "workflow_task");

            migrationBuilder.DropColumn(
                name: "AssigneeRoleId",
                table: "workflow_task");

            migrationBuilder.DropColumn(
                name: "AssigneeType",
                table: "workflow_task");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "workflow_task");

            migrationBuilder.DropColumn(
                name: "FailureReason",
                table: "workflow_instance");

            migrationBuilder.AlterColumn<long>(
                name: "AssigneeId",
                table: "workflow_task",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);
        }
    }
}
