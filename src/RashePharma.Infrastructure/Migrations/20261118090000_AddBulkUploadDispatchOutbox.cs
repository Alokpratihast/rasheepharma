using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using RashePharma.Infrastructure.Data;

#nullable disable

namespace RashePharma.Infrastructure.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20261118090000_AddBulkUploadDispatchOutbox")]
public partial class AddBulkUploadDispatchOutbox : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "BulkUploadDispatchOutbox",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                BulkUploadJobId = table.Column<int>(type: "int", nullable: false),
                EventType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                SchemaVersion = table.Column<int>(type: "int", nullable: false),
                PayloadJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                PublishedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                AttemptCount = table.Column<int>(type: "int", nullable: false),
                NextAttemptAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                LeaseUntilUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                LastError = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_BulkUploadDispatchOutbox", x => x.Id);
                table.ForeignKey(
                    name: "FK_BulkUploadDispatchOutbox_BulkUploadJobs_BulkUploadJobId",
                    column: x => x.BulkUploadJobId,
                    principalTable: "BulkUploadJobs",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "IX_BulkUploadDispatchOutbox_BulkUploadJobId_EventType",
            table: "BulkUploadDispatchOutbox",
            columns: new[] { "BulkUploadJobId", "EventType" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_BulkUploadDispatchOutbox_Status_NextAttemptAtUtc_CreatedAtUtc",
            table: "BulkUploadDispatchOutbox",
            columns: new[] { "Status", "NextAttemptAtUtc", "CreatedAtUtc" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "BulkUploadDispatchOutbox");
    }
}
