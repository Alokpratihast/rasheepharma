using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using RashePharma.Infrastructure.Data;

#nullable disable

namespace RashePharma.Infrastructure.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20261102190000_LinkEmailJobsToOutreachRecipients")]
public sealed class LinkEmailJobsToOutreachRecipients : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(
            name: "LeadOutreachRecipientId",
            table: "EmailJobs",
            type: "int",
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_EmailJobs_LeadOutreachRecipientId",
            table: "EmailJobs",
            column: "LeadOutreachRecipientId",
            unique: true,
            filter: "[LeadOutreachRecipientId] IS NOT NULL");

        migrationBuilder.AddForeignKey(
            name: "FK_EmailJobs_LeadOutreachRecipients_LeadOutreachRecipientId",
            table: "EmailJobs",
            column: "LeadOutreachRecipientId",
            principalTable: "LeadOutreachRecipients",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_EmailJobs_LeadOutreachRecipients_LeadOutreachRecipientId",
            table: "EmailJobs");

        migrationBuilder.DropIndex(
            name: "IX_EmailJobs_LeadOutreachRecipientId",
            table: "EmailJobs");

        migrationBuilder.DropColumn(
            name: "LeadOutreachRecipientId",
            table: "EmailJobs");
    }
}
