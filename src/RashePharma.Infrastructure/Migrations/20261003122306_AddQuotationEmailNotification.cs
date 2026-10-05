using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RashePharma.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddQuotationEmailNotification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EmailNotifications_OrderId_Type",
                table: "EmailNotifications");

            migrationBuilder.AlterColumn<int>(
                name: "OrderId",
                table: "EmailNotifications",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "QuotationId",
                table: "EmailNotifications",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmailNotifications_OrderId_Type",
                table: "EmailNotifications",
                columns: new[] { "OrderId", "Type" },
                unique: true,
                filter: "[OrderId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_EmailNotifications_QuotationId_Type",
                table: "EmailNotifications",
                columns: new[] { "QuotationId", "Type" },
                unique: true,
                filter: "[QuotationId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_EmailNotifications_Quotations_QuotationId",
                table: "EmailNotifications",
                column: "QuotationId",
                principalTable: "Quotations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmailNotifications_Quotations_QuotationId",
                table: "EmailNotifications");

            migrationBuilder.DropIndex(
                name: "IX_EmailNotifications_OrderId_Type",
                table: "EmailNotifications");

            migrationBuilder.DropIndex(
                name: "IX_EmailNotifications_QuotationId_Type",
                table: "EmailNotifications");

            migrationBuilder.DropColumn(
                name: "QuotationId",
                table: "EmailNotifications");

            migrationBuilder.AlterColumn<int>(
                name: "OrderId",
                table: "EmailNotifications",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmailNotifications_OrderId_Type",
                table: "EmailNotifications",
                columns: new[] { "OrderId", "Type" },
                unique: true);
        }
    }
}
