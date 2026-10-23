# Isolated local test environment

This setup is production-like infrastructure for development and automated integration checks. It is deliberately separate from Azure staging and production.

## Safety boundary

- SQL Server and Azurite run in local Docker containers.
- Host ports are bound to `127.0.0.1` only (`14330` for SQL Server, `10000` for Blob Storage).
- The app uses a database named `RashePharma_AutomatedTests` and the Azurite emulator; it does not read Azure connection strings.
- No production data is copied or used. Use synthetic test records and files only.
- The containers have no persistent Docker volumes. `down` removes their stored test data.
- `.env` is ignored by Git. Never put production, staging, Azure, or live payment credentials in it.

## Requirements

- Docker Desktop installed and running
- .NET 10 SDK and the `dotnet-ef` tool
- Node.js and npm for frontend checks

## Start the local API and dependencies

From the repository root:

```powershell
Copy-Item tests/environment/.env.example tests/environment/.env
notepad tests/environment/.env
```

Change `TEST_SQL_PASSWORD` to a strong local-only password. Keep `TEST_JWT_KEY` local and private as well. Then start the SQL Server container, Azurite, apply the current EF migrations to the disposable test database, and launch the API:

```powershell
.\tests\environment\start-local.ps1
```

The API runs at `http://localhost:5104`. The script intentionally fails if Docker is unavailable, the local password is unchanged, SQL does not start, or migrations fail. It uses a placeholder Stripe test key and never needs live Stripe credentials.

## Start the frontend against the local API

In another PowerShell window:

```powershell
cd frontend
$env:NEXT_PUBLIC_API_BASE_URL = 'http://localhost:5104/api'
npm run dev
```

Open `http://localhost:3000`. Create only disposable test data. For a bulk-upload run, use a synthetic workbook and test-only images, then verify the job status, imported products/variants, and row errors in the local database/API.

## Run automated tests

From the repository root:

```powershell
dotnet test RashePharma.slnx
cd frontend
npm test
npm run lint
```

The current API test project uses test doubles and does not yet run bulk-upload end-to-end cases against SQL Server/Azurite. The local dependencies above are ready for adding those integration tests; the existing test suites remain independent of Docker.

## Stop and discard the environment

```powershell
docker compose --env-file tests/environment/.env -f tests/environment/docker-compose.yml down
```

This removes the containers and their ephemeral database/blob contents. Remove `tests/environment/.env` when finished if you no longer need the local credentials.
