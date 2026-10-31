
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RashePharma.Domain.Entities.Email;

namespace RashePharma.Infrastructure.Data.Configurations.Email;

public sealed class EmailDeliveryEventConfiguration
    : IEntityTypeConfiguration<EmailDeliveryEvent>
{
    public void Configure(
        EntityTypeBuilder<EmailDeliveryEvent> builder)
    {
        builder.ToTable("EmailDeliveryEvents");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Provider)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.ProviderEventId)
            .HasMaxLength(200);

        builder.Property(x => x.EventType)
            .HasMaxLength(60)
            .IsRequired();

        builder.Property(x => x.RawPayloadJson)
            .HasColumnType("nvarchar(max)");

        // A provider event must not be stored twice.
        builder.HasIndex(x => new
        {
            x.Provider,
            x.ProviderEventId
        })
        .IsUnique()
        .HasFilter("[ProviderEventId] IS NOT NULL");

        // Retrieve event history for a specific email job.
        builder.HasIndex(x => new
        {
            x.EmailJobId,
            x.OccurredAtUtc
        });

        builder.HasOne(x => x.EmailJob)
            .WithMany()
            .HasForeignKey(x => x.EmailJobId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
