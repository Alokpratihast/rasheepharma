using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RashePharma.Domain.Entities.BulkUpload;
namespace RashePharma.Infrastructure.Data.Configurations;
public sealed class BulkUploadDispatchOutboxConfiguration : IEntityTypeConfiguration<BulkUploadDispatchOutbox> {
 public void Configure(EntityTypeBuilder<BulkUploadDispatchOutbox> b) {
 b.ToTable("BulkUploadDispatchOutbox"); b.HasKey(x=>x.Id); b.Property(x=>x.EventType).HasMaxLength(100).IsRequired();
 b.Property(x=>x.PayloadJson).HasColumnType("nvarchar(max)").IsRequired(); b.Property(x=>x.Status).HasMaxLength(30).IsRequired();
 b.Property(x=>x.LastError).HasMaxLength(2000); b.Property(x=>x.RowVersion).IsRowVersion();
 b.HasIndex(x=>new{x.BulkUploadJobId,x.EventType}).IsUnique(); b.HasIndex(x=>new{x.Status,x.NextAttemptAtUtc,x.CreatedAtUtc});
 b.HasOne(x=>x.BulkUploadJob).WithMany().HasForeignKey(x=>x.BulkUploadJobId).OnDelete(DeleteBehavior.Restrict);
 }}