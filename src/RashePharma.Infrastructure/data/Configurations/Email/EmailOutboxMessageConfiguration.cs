
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RashePharma.Domain.Entities.Email;

namespace RashePharma.Infrastructure.Data.Configurations.Email;

public sealed class EmailOutboxMessageConfiguration
    : IEntityTypeConfiguration<EmailOutboxMessage>
{
    public void Configure(
        EntityTypeBuilder<EmailOutboxMessage> builder)
    {
        builder.ToTable("EmailOutboxMessages");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.EventType)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.SchemaVersion)
            .IsRequired();

        builder.Property(x => x.PayloadJson)
            .HasColumnType("nvarchar(max)")
            .IsRequired();

        builder.Property(x => x.Status)
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(x => x.LastError)
            .HasMaxLength(2000);

        builder.Property(x => x.RowVersion)
            .IsRowVersion();

        // One initial EmailRequested outbox event per email job.
        builder.HasIndex(x => new { x.EmailJobId, x.EventType })
            .IsUnique()
            .HasFilter("[EventType] = 'EmailRequested'");

        builder.HasIndex(x => new
        {
            x.Status,
            x.NextAttemptAtUtc,
            x.CreatedAtUtc
        });

        builder.HasOne<EmailJob>()
            .WithMany()
            .HasForeignKey(x => x.EmailJobId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
