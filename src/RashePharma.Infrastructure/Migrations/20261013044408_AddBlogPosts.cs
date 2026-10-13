using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RashePharma.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBlogPosts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BlogPosts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Excerpt = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", maxLength: 100000, nullable: false),
                    CoverImageUrl = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    AuthorName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    ReviewerName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    ReviewedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PublishedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MetaTitle = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: true),
                    MetaDescription = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: true),
                    CanonicalUrl = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    OgTitle = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: true),
                    OgDescription = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: true),
                    OgImageUrl = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    IsIndexable = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlogPosts", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BlogPosts_Slug",
                table: "BlogPosts",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BlogPosts_Status_PublishedAtUtc",
                table: "BlogPosts",
                columns: new[] { "Status", "PublishedAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BlogPosts");
        }
    }
}
