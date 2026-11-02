# RashePharma Email Worker

This separately deployable .NET host dispatches SQL Outbox messages to Azure Service Bus and consumes transactional and bulk email queues. It uses the same SQL database and Brevo SMTP configuration as the API.

## Required settings

Provide these through the host's secret store or environment variables; do not commit credentials:

- `ConnectionStrings__DefaultConnection`
- Either `AzureServiceBus__ConnectionString` or `AzureServiceBus__FullyQualifiedNamespace` (with managed identity)
- `Brevo__SmtpHost`, `Brevo__SmtpPort`, `Brevo__SmtpUsername`, `Brevo__SmtpPassword`, `Brevo__FromEmail`, and `Brevo__FromName`
- `Jwt__Key` is only needed by the API recipient producer to make unsubscribe and reply links.

## Local run

Run the worker only against a dedicated local/staging database and non-production queue/provider credentials. Starting it will process real jobs already present in those configured queues.

```powershell
$env:ConnectionStrings__DefaultConnection = "<local-or-staging-sql-connection>"
$env:AzureServiceBus__ConnectionString = "<non-production-service-bus-connection>"
$env:Brevo__SmtpHost = "<staging-smtp-host>"
$env:Brevo__SmtpPort = "587"
$env:Brevo__SmtpUsername = "<staging-smtp-user>"
$env:Brevo__SmtpPassword = "<staging-smtp-password>"
$env:Brevo__FromEmail = "<verified-sender>"
$env:Brevo__FromName = "Rashe Lifesciences"
dotnet run --project src/RashePharma.EmailWorker
```

Email delivery status `ProviderAccepted` means the SMTP provider accepted the message; actual delivery, bounce, open, and click states require provider webhooks.
