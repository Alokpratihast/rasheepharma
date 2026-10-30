
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RashePharma.Domain.Entities.Email;

namespace RashePharma.Infrastructure.Data.Configurations.Email;

public sealed class EmailQuotaReservationConfiguration
    : IEntityTypeConfiguration<EmailQuotaReservation>
{
    public void Configure(
        EntityTypeBuilder<EmailQuotaReservation> builder)
    {
        builder.ToTable("EmailQuotaReservations");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Status)
            .HasMaxLength(30)
            .IsRequired();

        builder.HasIndex(x => new
        {
            x.EmailJobId,
            x.AttemptNumber
        })
        .IsUnique();

        builder.HasIndex(x => new
        {
            x.QuotaBucketId,
            x.Status
        });

        builder.HasOne(x => x.EmailJob)
            .WithMany()
            .HasForeignKey(x => x.EmailJobId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.QuotaBucket)
            .WithMany()
            .HasForeignKey(x => x.QuotaBucketId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
