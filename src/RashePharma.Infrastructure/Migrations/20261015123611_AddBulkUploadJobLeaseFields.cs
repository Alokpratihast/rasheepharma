using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RashePharma.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBulkUploadJobLeaseFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AttemptCount",
                table: "BulkUploadJobs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ClaimToken",
                table: "BulkUploadJobs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "HeartbeatAtUtc",
                table: "BulkUploadJobs",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LeaseExpiresAtUtc",
                table: "BulkUploadJobs",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AttemptCount",
                table: "BulkUploadJobs");

            migrationBuilder.DropColumn(
                name: "ClaimToken",
                table: "BulkUploadJobs");

            migrationBuilder.DropColumn(
                name: "HeartbeatAtUtc",
                table: "BulkUploadJobs");

            migrationBuilder.DropColumn(
                name: "LeaseExpiresAtUtc",
                table: "BulkUploadJobs");
        }
    }
}
