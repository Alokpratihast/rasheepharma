
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RashePharma.Domain.Entities.Email;

namespace RashePharma.Infrastructure.Data.Configurations.Email;

public sealed class EmailQuotaBucketConfiguration
    : IEntityTypeConfiguration<EmailQuotaBucket>
{
    public void Configure(
        EntityTypeBuilder<EmailQuotaBucket> builder)
    {
        builder.ToTable("EmailQuotaBuckets", table =>
        {
            table.HasCheckConstraint(
                "CK_EmailQuotaBuckets_ValidWindow",
                "[WindowEndUtc] > [WindowStartUtc]");

            table.HasCheckConstraint(
                "CK_EmailQuotaBuckets_ValidCounts",
                "[DailyLimit] >= 0 AND [ReservedCount] >= 0 AND [ReservedCount] <= [DailyLimit]");
        });

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Provider)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.RowVersion)
            .IsRowVersion();

        // One quota bucket per provider per time window.
        builder.HasIndex(x => new
        {
            x.Provider,
            x.WindowStartUtc,
            x.WindowEndUtc
        })
        .IsUnique();

        builder.HasIndex(x => new
        {
            x.Provider,
            x.WindowEndUtc
        });
    }
}
