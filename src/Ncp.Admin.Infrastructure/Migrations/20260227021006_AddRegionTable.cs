using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ncp.Admin.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRegionTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "region",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ParentId = table.Column<long>(type: "bigint", nullable: false),
                    Level = table.Column<int>(type: "integer", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_region", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_region_Level",
                table: "region",
                column: "Level");

            migrationBuilder.CreateIndex(
                name: "IX_region_ParentId",
                table: "region",
                column: "ParentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "region");
        }
    }
}
