using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using RashePharma.Infrastructure.Data;

#nullable disable

namespace RashePharma.Infrastructure.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20261125100000_AddUniqueRoleName")]
public partial class AddUniqueRoleName : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            IF EXISTS
            (
                SELECT [Name]
                FROM [Roles]
                GROUP BY [Name]
                HAVING COUNT(*) > 1
            )
            BEGIN
                THROW 51000,
                    'Cannot create the unique Roles.Name index because duplicate role names already exist. Resolve duplicates before retrying.',
                    1;
            END;

            IF EXISTS
            (
                SELECT 1
                FROM [Roles]
                WHERE LEN([Name]) > 50
            )
            BEGIN
                THROW 51001,
                    'Cannot limit Roles.Name to 50 characters because an existing role name is longer. Resolve the data before retrying.',
                    1;
            END;
            """);

        migrationBuilder.AlterColumn<string>(
            name: "Name",
            table: "Roles",
            type: "nvarchar(50)",
            maxLength: 50,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(max)");

        migrationBuilder.CreateIndex(
            name: "IX_Roles_Name",
            table: "Roles",
            column: "Name",
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_Roles_Name",
            table: "Roles");

        migrationBuilder.AlterColumn<string>(
            name: "Name",
            table: "Roles",
            type: "nvarchar(max)",
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(50)",
            oldMaxLength: 50);
    }
}
