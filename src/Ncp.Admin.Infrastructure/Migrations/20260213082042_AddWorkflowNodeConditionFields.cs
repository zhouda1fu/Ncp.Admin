using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ncp.Admin.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkflowNodeConditionFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ConditionExpression",
                table: "workflow_node",
                type: "varchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "FalseNextNodeName",
                table: "workflow_node",
                type: "varchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "TrueNextNodeName",
                table: "workflow_node",
                type: "varchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConditionExpression",
                table: "workflow_node");

            migrationBuilder.DropColumn(
                name: "FalseNextNodeName",
                table: "workflow_node");

            migrationBuilder.DropColumn(
                name: "TrueNextNodeName",
                table: "workflow_node");
        }
    }
}
