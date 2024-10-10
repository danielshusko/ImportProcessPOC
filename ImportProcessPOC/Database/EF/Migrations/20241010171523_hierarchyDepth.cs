using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ImportProcessPOC.Migrations
{
    /// <inheritdoc />
    public partial class hierarchyDepth : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "HierarchyDepth",
                schema: "importProcessPoc",
                table: "ImportJobLine",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HierarchyDepth",
                schema: "importProcessPoc",
                table: "ImportJobLine");
        }
    }
}
