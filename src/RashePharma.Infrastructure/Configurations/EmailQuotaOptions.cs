namespace RashePharma.Infrastructure.Configurations;

public sealed class EmailQuotaOptions
{
    public const string SectionName = "EmailQuota";

    public int OutreachDailyLimit { get; set; } = 300;
}
