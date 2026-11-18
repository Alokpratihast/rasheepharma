# Bulk upload worker deployment

The worker is a separate .NET 10 container. Build from the repository root:

`docker build -f src/RashePharma.BulkUploadWorker/Dockerfile -t rashepharma-bulk-upload-worker:<tag> .`

Required runtime configuration:

- `ConnectionStrings__DefaultConnection`: SQL Server connection string.
- `ConnectionStrings__AzureStorage`: Azure Storage connection string.
- `AzureServiceBus__ConnectionString` **or** `AzureServiceBus__FullyQualifiedNamespace`.
- `AzureServiceBus__BulkUploadQueueName`: dedicated queue name; default `bulk-upload`.

Create the Service Bus queue before starting the worker. Do not point this consumer at either email queue.

## Azure Container Apps scaling target

Configure the Container App with a Service Bus queue scale rule:

- minimum replicas: `0`
- maximum replicas: start with `3`, then tune from measured processing duration and SQL capacity
- queue: `bulk-upload`
- target messages per replica: start with `1`
- polling interval: `30` seconds
- cooldown: `300` seconds

The Service Bus trigger identity must have **Azure Service Bus Data Receiver** on the queue. The API dispatcher identity must have **Azure Service Bus Data Sender**. The worker identity also needs SQL and Blob Storage access. Prefer managed identities and Entra authentication in Azure; use connection-string secrets only for local development or where identity-based authentication has not yet been configured.

Do not deploy this template to production as part of this feature branch. First validate the worker locally and in a non-production environment, create the queue and identities, then apply the Container Apps scale rule to the approved staging app.
