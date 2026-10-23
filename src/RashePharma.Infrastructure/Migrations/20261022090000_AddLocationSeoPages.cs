using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using RashePharma.Infrastructure.Data;

#nullable disable

namespace RashePharma.Infrastructure.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20261022090000_AddLocationSeoPages")]
public partial class AddLocationSeoPages : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "LocationSeoPages",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Country = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                CountryCode = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                CountrySlug = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                QuoteCurrency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                City = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                Region = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                CitySlug = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                MetaTitle = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                MetaDescription = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: false),
                Heading = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                Intro = table.Column<string>(type: "nvarchar(3000)", maxLength: 3000, nullable: false),
                BuyerFocus = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                IsPublished = table.Column<bool>(type: "bit", nullable: false),
                IsIndexable = table.Column<bool>(type: "bit", nullable: false),
                CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_LocationSeoPages", x => x.Id));

        migrationBuilder.CreateIndex(
            name: "IX_LocationSeoPages_CountrySlug_CitySlug",
            table: "LocationSeoPages",
            columns: new[] { "CountrySlug", "CitySlug" },
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
        => migrationBuilder.DropTable(name: "LocationSeoPages");
}
