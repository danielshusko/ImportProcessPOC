using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ImportProcessPOC.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "importProcessPoc");

            migrationBuilder.CreateTable(
                name: "ImportJob",
                schema: "importProcessPoc",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Importer = table.Column<string>(type: "text", nullable: false),
                    ItemCount = table.Column<int>(type: "integer", nullable: false),
                    IsOrdered = table.Column<bool>(type: "boolean", nullable: false),
                    StartDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImportJob", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Item",
                schema: "importProcessPoc",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ParentId = table.Column<int>(type: "integer", nullable: true),
                    Code = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Item", x => new { x.Id, x.TenantId });
                    table.ForeignKey(
                        name: "FK_Item_Item_ParentId_TenantId",
                        columns: x => new { x.ParentId, x.TenantId },
                        principalSchema: "importProcessPoc",
                        principalTable: "Item",
                        principalColumns: new[] { "Id", "TenantId" });
                });

            migrationBuilder.CreateTable(
                name: "ImportJobHeader",
                schema: "importProcessPoc",
                columns: table => new
                {
                    JobId = table.Column<int>(type: "integer", nullable: false),
                    Index = table.Column<int>(type: "integer", nullable: false),
                    Header = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImportJobHeader", x => new { x.JobId, x.Index });
                    table.ForeignKey(
                        name: "FK_ImportJobHeader_ImportJob_JobId",
                        column: x => x.JobId,
                        principalSchema: "importProcessPoc",
                        principalTable: "ImportJob",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ImportJobLine",
                schema: "importProcessPoc",
                columns: table => new
                {
                    JobId = table.Column<int>(type: "integer", nullable: false),
                    Index = table.Column<int>(type: "integer", nullable: false),
                    Id = table.Column<int>(type: "integer", nullable: false),
                    ParentId = table.Column<int>(type: "integer", nullable: true),
                    Line = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImportJobLine", x => new { x.JobId, x.Index });
                    table.ForeignKey(
                        name: "FK_ImportJobLine_ImportJob_JobId",
                        column: x => x.JobId,
                        principalSchema: "importProcessPoc",
                        principalTable: "ImportJob",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ImportJobLineQueue",
                schema: "importProcessPoc",
                columns: table => new
                {
                    JobId = table.Column<int>(type: "integer", nullable: false),
                    Index = table.Column<int>(type: "integer", nullable: false),
                    Id = table.Column<int>(type: "integer", nullable: false),
                    ParentId = table.Column<int>(type: "integer", nullable: true),
                    Line = table.Column<string>(type: "text", nullable: false),
                    IsProcessed = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImportJobLineQueue", x => new { x.JobId, x.Index });
                    table.ForeignKey(
                        name: "FK_ImportJobLineQueue_ImportJob_JobId",
                        column: x => x.JobId,
                        principalSchema: "importProcessPoc",
                        principalTable: "ImportJob",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ImportJobSpan",
                schema: "importProcessPoc",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    JobId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    StartDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImportJobSpan", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ImportJobSpan_ImportJob_JobId",
                        column: x => x.JobId,
                        principalSchema: "importProcessPoc",
                        principalTable: "ImportJob",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ImportJobLineQueue_ParentId_IsProcessed",
                schema: "importProcessPoc",
                table: "ImportJobLineQueue",
                columns: new[] { "ParentId", "IsProcessed" });

            migrationBuilder.CreateIndex(
                name: "IX_ImportJobSpan_JobId",
                schema: "importProcessPoc",
                table: "ImportJobSpan",
                column: "JobId");

            migrationBuilder.CreateIndex(
                name: "IX_Item_ParentId_TenantId",
                schema: "importProcessPoc",
                table: "Item",
                columns: new[] { "ParentId", "TenantId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ImportJobHeader",
                schema: "importProcessPoc");

            migrationBuilder.DropTable(
                name: "ImportJobLine",
                schema: "importProcessPoc");

            migrationBuilder.DropTable(
                name: "ImportJobLineQueue",
                schema: "importProcessPoc");

            migrationBuilder.DropTable(
                name: "ImportJobSpan",
                schema: "importProcessPoc");

            migrationBuilder.DropTable(
                name: "Item",
                schema: "importProcessPoc");

            migrationBuilder.DropTable(
                name: "ImportJob",
                schema: "importProcessPoc");
        }
    }
}
