using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using RashePharma.Infrastructure.Data;
#nullable disable
namespace RashePharma.Infrastructure.Migrations;
[DbContext(typeof(ApplicationDbContext))]
[Migration("20261118090000_AddBulkUploadDispatchOutbox")]
public partial class AddBulkUploadDispatchOutbox : Migration
{
 protected override void Up(MigrationBuilder m)
 {
  m.CreateTable(name:"BulkUploadDispatchOutbox",columns:t=>new{
   Id=t.Column<Guid>(type:"uniqueidentifier",nullable:false),BulkUploadJobId=t.Column<int>(type:"int",nullable:false),
   EventType=t.Column<string>(type:"nvarchar(100)",maxLength:100,nullable:false),SchemaVersion=t.Column<int>(type:"int",nullable:false),
   PayloadJson=t.Column<string>(type:"nvarchar(max)",nullable:false),Status=t.Column<string>(type:"nvarchar(30)",maxLength:30,nullable:false),
   CreatedAtUtc=t.Column<DateTime>(type:"datetime2",nullable:false),PublishedAtUtc=t.Column<DateTime>(type:"datetime2",nullable:true),
   AttemptCount=t.Column<int>(type:"int",nullable:false),NextAttemptAtUtc=t.Column<DateTime>(type:"datetime2",nullable:true),
   LeaseUntilUtc=t.Column<DateTime>(type:"datetime2",nullable:true),LastError=t.Column<string>(type:"nvarchar(2000)",maxLength:2000,nullable:true),
   RowVersion=t.Column<byte[]>(type:"rowversion",rowVersion:true,nullable:false)
  },constraints:t=>{t.PrimaryKey("PK_BulkUploadDispatchOutbox",x=>x.Id);t.ForeignKey("FK_BulkUploadDispatchOutbox_BulkUploadJobs_BulkUploadJobId",x=>x.BulkUploadJobId,"BulkUploadJobs","Id",onDelete:ReferentialAction.Restrict);});
  m.CreateIndex("IX_BulkUploadDispatchOutbox_BulkUploadJobId_EventType","BulkUploadDispatchOutbox",new[]{"BulkUploadJobId","EventType"},unique:true);
  m.CreateIndex("IX_BulkUploadDispatchOutbox_Status_NextAttemptAtUtc_CreatedAtUtc","BulkUploadDispatchOutbox",new[]{"Status","NextAttemptAtUtc","CreatedAtUtc"});
 }
 protected override void Down(MigrationBuilder m)=>m.DropTable("BulkUploadDispatchOutbox");
}