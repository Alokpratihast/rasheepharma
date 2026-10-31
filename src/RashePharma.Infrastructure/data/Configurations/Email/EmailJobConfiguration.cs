
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RashePharma.Domain.Entities.Email;

namespace RashePharma.Infrastructure.Data.Configurations.Email;

public sealed class EmailJobConfiguration
    : IEntityTypeConfiguration<EmailJob>
{
    public void Configure(EntityTypeBuilder<EmailJob> builder)
    {
        builder.ToTable("EmailJobs");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.IdempotencyKey)
            .HasMaxLength(200)
            .IsRequired();

        builder.HasIndex(x => x.IdempotencyKey)
            .IsUnique();

        builder.Property(x => x.Category)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(40)
            .IsRequired();

        builder.Property(x => x.DeliveryStatus)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(x => x.EmailType)
            .HasMaxLength(60)
            .IsRequired();

        builder.Property(x => x.RecipientEmail)
            .HasMaxLength(320)
            .IsRequired();

        builder.Property(x => x.TemplateKey)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.PayloadJson)
            .HasColumnType("nvarchar(max)")
            .IsRequired();

        builder.Property(x => x.ProviderMessageId)
            .HasMaxLength(200);

        builder.HasIndex(x => x.ProviderMessageId)
            .IsUnique()
            .HasFilter("[ProviderMessageId] IS NOT NULL");

        builder.Property(x => x.CorrelationId)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.LastErrorCode)
            .HasMaxLength(100);

        builder.Property(x => x.LastErrorMessage)
            .HasMaxLength(2000);

        builder.Property(x => x.RowVersion)
            .IsRowVersion();

        builder.HasIndex(x => new
        {
            x.Status,
            x.NextAttemptAtUtc,
            x.CreatedAtUtc
        });

        builder.HasIndex(x => new
        {
            x.Category,
            x.Status,
            x.CreatedAtUtc
        });
    }
}
