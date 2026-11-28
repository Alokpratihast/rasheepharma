# VPS staging deployment

This is a staging template for the GoDaddy VPS. It keeps SQL Server and the existing RabbitMQ service external to this Compose project. The app containers bind only to loopback; host Nginx is the public entry point.

## Required host setup

- Ubuntu 24.04, Docker Engine with Compose, and host Nginx.
- SQL Server Express reachable from Docker at the private bridge gateway (`172.18.0.1:1433`); firewall/bind SQL Server so it is not publicly reachable.
- The existing Docker network `rasheepharma-network` must already exist with gateway `172.18.0.1`; Compose reuses it as an external network rather than creating an overlapping `172.18.0.0/16` network. Verify it with `docker network inspect rasheepharma-network` before deployment.
- Existing RabbitMQ reachable from the containers via `host.docker.internal:5672`. Confirm its Docker network, bind address, credentials and durable storage before starting this stack. Do not recreate or modify the current RabbitMQ container as part of this template.
- A separate staging database, uploads directory, RabbitMQ queue names, secrets and hostname. Do not use production database credentials for staging.
- Place the brochure PDF at `/opt/rasheepharma/staging/uploads/Rashe_Lifesciences_Client_Brochure_v2.pdf`.
- Existing Azure-hosted blog cover images must be copied into `blogs/` and their staging database URLs changed to `local://blogs/<stored-name>`; otherwise the private blog image endpoint cannot read them. Copy product images into `products/` and update any rows still using Blob URLs to `local://products/<stored-name>` if you want those assets served from this VPS. Keep a source-to-destination filename map and verify the copied files before changing the database.

## Prepare staging

1. Create `/opt/rasheepharma/staging/uploads` and set its owner/permissions for the container processes.
2. Copy `.env.example` to `.env`, fill every `CHANGE_ME`, then restrict it with `chmod 600 .env`.
3. Set the `staging.rashelifesciences.com` DNS record to the VPS and issue a TLS certificate. Configure `nginx-staging.conf` with the certificate paths, then validate/reload host Nginx.
4. Build the app images with `docker compose -f compose.yaml build api frontend bulk-upload-worker db-migrator`.
5. Run the one-shot schema migrator only after confirming the staging database and backup: `docker compose -f compose.yaml --profile migration run --rm db-migrator`.
6. Start app services with `docker compose -f compose.yaml up -d api frontend bulk-upload-worker`.
7. Migrate and verify existing blog and product image files before switching their database URLs, as described above.
8. Check `https://staging.rashelifesciences.com/health`, frontend/API flows, images, blog, quotation brochure, Stripe test mode and bulk-upload worker before considering production cutover.

The API outbox dispatcher checks for lost/dead-lettered deliveries every 60 seconds when no earlier retry is scheduled. This is a lightweight recovery query against the VPS-hosted SQL Server, not a serverless database auto-pause workload.\n\nThe exact RabbitMQ connectivity and SQL Server bridge binding must be confirmed on the VPS first. The staging domain, TLS certificate and secrets are placeholders; this repository change does not configure the server or change DNS.
