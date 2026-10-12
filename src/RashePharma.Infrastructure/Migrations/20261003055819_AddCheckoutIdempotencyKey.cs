using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RashePharma.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCheckoutIdempotencyKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Payments_OrderId",
                table: "Payments");

            migrationBuilder.AddColumn<string>(
                name: "CheckoutIdempotencyKey",
                table: "Payments",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Payments_CheckoutIdempotencyKey",
                table: "Payments",
                column: "CheckoutIdempotencyKey",
                unique: true,
                filter: "[CheckoutIdempotencyKey] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_OrderId",
                table: "Payments",
                column: "OrderId",
                unique: true,
                filter: "[Status] IN ('Pending', 'Failed')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Payments_CheckoutIdempotencyKey",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Payments_OrderId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "CheckoutIdempotencyKey",
                table: "Payments");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_OrderId",
                table: "Payments",
                column: "OrderId");
        }
    }
}
