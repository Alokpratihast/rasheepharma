using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RashePharma.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProductCatalogFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "ProductVariants",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MOQ",
                table: "ProductVariants",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UnitType",
                table: "ProductVariants",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BrandName",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Currency",
                table: "ProductVariants");

            migrationBuilder.DropColumn(
                name: "MOQ",
                table: "ProductVariants");

            migrationBuilder.DropColumn(
                name: "UnitType",
                table: "ProductVariants");

            migrationBuilder.DropColumn(
                name: "BrandName",
                table: "Products");
        }
    }
}
