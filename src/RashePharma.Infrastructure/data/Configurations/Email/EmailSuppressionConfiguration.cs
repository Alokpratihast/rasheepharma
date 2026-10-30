
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RashePharma.Domain.Entities.Email;

namespace RashePharma.Infrastructure.Data.Configurations.Email;

public sealed class EmailSuppressionConfiguration
    : IEntityTypeConfiguration<EmailSuppression>
{
    public void Configure(
        EntityTypeBuilder<EmailSuppression> builder)
    {
        builder.ToTable("EmailSuppressions", table =>
        {
            table.HasCheckConstraint(
                "CK_EmailSuppressions_Type",
                "[SuppressionType] IN ('Unsubscribed', 'HardBounce', 'Complaint')");
        });

        builder.HasKey(x => x.Id);

        builder.Property(x => x.EmailNormalized)
            .HasMaxLength(320)
            .IsRequired();

        builder.Property(x => x.SuppressionType)
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(x => x.Source)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Reason)
            .HasMaxLength(500);

        // Ignore the calculated property; it is not a database column.
        builder.Ignore(x => x.IsActive);

        // Supports active-suppression lookups.
        builder.HasIndex(x => new
        {
            x.EmailNormalized,
            x.SuppressionType
        })
        .IsUnique()
        .HasFilter("[LiftedAtUtc] IS NULL");
    }
}
