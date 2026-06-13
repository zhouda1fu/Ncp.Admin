using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ncp.Admin.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemovePermissionPreset : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "permission_preset");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "permission_preset",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, comment: "权限预设包标识"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "创建时间"),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false, comment: "说明"),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, comment: "是否启用"),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false, comment: "是否为系统默认配置包"),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, comment: "是否软删"),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "预设名称"),
                    PermissionCodesJson = table.Column<string>(type: "text", nullable: false, comment: "权限码 JSON 数组"),
                    SortOrder = table.Column<int>(type: "integer", nullable: false, comment: "排序"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "更新时间")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_permission_preset", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_permission_preset_Name",
                table: "permission_preset",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_permission_preset_SortOrder",
                table: "permission_preset",
                column: "SortOrder");
        }
    }
}
