using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RashePharma.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SyncAllPendingChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EnquiryId",
                table: "EmailNotifications",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmailNotifications_EnquiryId_Type",
                table: "EmailNotifications",
                columns: new[] { "EnquiryId", "Type" },
                unique: true,
                filter: "[EnquiryId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_EmailNotifications_Enquiries_EnquiryId",
                table: "EmailNotifications",
                column: "EnquiryId",
                principalTable: "Enquiries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmailNotifications_Enquiries_EnquiryId",
                table: "EmailNotifications");

            migrationBuilder.DropIndex(
                name: "IX_EmailNotifications_EnquiryId_Type",
                table: "EmailNotifications");

            migrationBuilder.DropColumn(
                name: "EnquiryId",
                table: "EmailNotifications");
        }
    }
}
