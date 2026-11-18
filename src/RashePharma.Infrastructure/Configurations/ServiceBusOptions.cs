namespace RashePharma.Infrastructure.Configurations;

public sealed class ServiceBusOptions
{
    public const string SectionName = "AzureServiceBus";

    public string ConnectionString { get; set; } = string.Empty;

    public string FullyQualifiedNamespace { get; set; } = string.Empty;

    public string TransactionalQueueName { get; set; } =
        "email-transactional";

    public string BulkQueueName { get; set; } =
        "email-bulk";

    public string TestQueueName { get; set; } =
    "email-connectivity-test";
}