# Bulk upload worker

Bulk upload processing runs as a separate .NET 10 container and consumes work from RabbitMQ. SQL remains the durable transactional outbox and job state store; local uploads are shared with the API through the same persistent host directory.

For VPS staging, follow [the VPS deployment guide](../vps/README.md). The Compose file builds the worker from `src/RashePharma.BulkUploadWorker/Dockerfile`, supplies the SQL Server and RabbitMQ settings from the VPS-only `.env`, and mounts the shared uploads root.

The worker preserves the SQL claim lease, heartbeat and fencing behavior. RabbitMQ delivery uses durable queues, manual acknowledgements and bounded prefetch. Configure distinct staging queue names and credentials in staging, and verify RabbitMQ reachability from the Docker bridge before starting the worker.
