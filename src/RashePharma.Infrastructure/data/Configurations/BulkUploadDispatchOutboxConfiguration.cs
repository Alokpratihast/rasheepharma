using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RashePharma.Domain.Entities.BulkUpload;

namespace RashePharma.Infrastructure.Data.Configurations;

public sealed class BulkUploadDispatchOutboxConfiguration : IEntityTypeConfiguration<BulkUploadDispatchOutbox>
{
    public void Configure(EntityTypeBuilder<BulkUploadDispatchOutbox> builder)
    {
        builder.ToTable("BulkUploadDispatchOutbox");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.EventType).HasMaxLength(100).IsRequired();
        builder.Property(x => x.PayloadJson).HasColumnType("nvarchar(max)").IsRequired();
        builder.Property(x => x.Status).HasMaxLength(30).IsRequired();
        builder.Property(x => x.LastError).HasMaxLength(2000);
        builder.Property(x => x.RowVersion).IsRowVersion();
        builder.HasIndex(x => new { x.BulkUploadJobId, x.EventType }).IsUnique();
        builder.HasIndex(x => new { x.Status, x.NextAttemptAtUtc, x.CreatedAtUtc });
        builder.HasOne(x => x.BulkUploadJob).WithMany().HasForeignKey(x => x.BulkUploadJobId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
