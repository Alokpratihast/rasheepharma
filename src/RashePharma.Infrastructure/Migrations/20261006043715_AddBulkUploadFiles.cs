using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RashePharma.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBulkUploadFiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BulkUploadFiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BulkUploadJobId = table.Column<int>(type: "int", nullable: false),
                    OriginalFileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BlobName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BulkUploadFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BulkUploadFiles_BulkUploadJobs_BulkUploadJobId",
                        column: x => x.BulkUploadJobId,
                        principalTable: "BulkUploadJobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BulkUploadFiles_BulkUploadJobId",
                table: "BulkUploadFiles",
                column: "BulkUploadJobId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BulkUploadFiles");
        }
    }
}
