namespace RashePharma.Infrastructure.Configurations;

public sealed class RabbitMqOptions
{
    public const string SectionName = "RabbitMQ";

    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string VirtualHost { get; set; } = "/";
    public string Username { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string BulkUploadQueueName { get; set; } = "bulk-upload";
    public string DeadLetterExchangeName { get; set; } = "bulk-upload.dlx";
    public string DeadLetterQueueName { get; set; } = "bulk-upload.dead-letter";
}
