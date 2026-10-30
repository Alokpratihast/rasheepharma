using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace RashePharma.Api.Tests;

public class AzureServiceBusConnectivityTests
{
    [Fact]
    public async Task SendOnlyPolicy_ShouldPublishMessageToTestQueue()
    {
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<Program>()
            .Build();

        var connectionString =
            configuration["AzureServiceBus:ConnectionString"];

        Assert.False(
            string.IsNullOrWhiteSpace(connectionString),
            "Azure Service Bus connection string is missing.");

        await using var client =
            new ServiceBusClient(connectionString);

        await using var sender =
            client.CreateSender("email-connectivity-test");

        var message = new ServiceBusMessage(
            BinaryData.FromString(
                """{"event":"connectivity-test","source":"RashePharma"}"""))
        {
            MessageId = Guid.NewGuid().ToString("N"),
            ContentType = "application/json",
            Subject = "ConnectivityTest"
        };

        await sender.SendMessageAsync(message);
    }
}