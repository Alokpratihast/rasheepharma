# Bulk upload worker deployment

The worker is a separate .NET 10 container. Build from the repository root:

`docker build -f src/RashePharma.BulkUploadWorker/Dockerfile -t rashepharma-bulk-upload-worker:<tag> .`

For local execution, configure these environment variables and run
`dotnet run --project src/RashePharma.BulkUploadWorker/RashePharma.BulkUploadWorker.csproj`:

- `ConnectionStrings__DefaultConnection`: SQL Server connection string.
- `ConnectionStrings__AzureStorage`: Azure Storage connection string (the current Blob Storage services use connection-string authentication).
- `AzureServiceBus__FullyQualifiedNamespace`: Service Bus namespace FQDN, for example `<namespace>.servicebus.windows.net`, or use `AzureServiceBus__ConnectionString` for local development.
- `AzureServiceBus__BulkUploadQueueName`: dedicated queue name; default `bulk-upload`.

Create the Service Bus queue before starting the worker. Do not point this consumer at either email queue.

## Azure Container Apps scale rule

The intended non-production scaling profile is:

- minimum replicas: `0`
- maximum replicas: `3` initially; tune using measured processing duration and SQL capacity
- scaler: `azure-servicebus`
- queue: `bulk-upload`
- target messages per replica: `1`
- polling interval: `30` seconds
- cooldown: `300` seconds

For a system-assigned managed identity, enable identity on the Container App and configure the scale rule (replace placeholders):

```bash
az containerapp identity assign --name <WORKER_APP> --resource-group <RESOURCE_GROUP> --system-assigned

az containerapp update \
  --name <WORKER_APP> \
  --resource-group <RESOURCE_GROUP> \
  --min-replicas 0 \
  --max-replicas 3 \
  --scale-rule-name bulk-upload-queue \
  --scale-rule-type azure-servicebus \
  --scale-rule-metadata "queueName=bulk-upload" "namespace=<SERVICE_BUS_NAMESPACE>" "messageCount=1" \
  --scale-rule-identity system
```

Grant the worker identity **Azure Service Bus Data Receiver** on the queue and the API identity **Azure Service Bus Data Sender**. Configure `AzureServiceBus__FullyQualifiedNamespace` in both apps so the SDK uses Entra authentication. The current Blob Storage integration still requires `ConnectionStrings__AzureStorage`; keep it in Container Apps secrets, not in source control.

Do not deploy this template to production as part of this feature branch. First validate the worker locally and in a non-production environment, create the queue and identities, then apply the Container Apps scale rule to the approved staging app.
