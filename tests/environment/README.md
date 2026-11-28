# Isolated local test environment

This setup provides isolated local infrastructure for development and automated integration checks.

## Safety boundary

- SQL Server runs in a local Docker container; uploaded test files stay under the system temporary directory.
- The SQL Server host port is bound to `127.0.0.1:14330` only.
- The app uses a database named `RashePharma_AutomatedTests` and local filesystem storage.
- No production data is copied or used. Use synthetic test records and files only.
- The containers have no persistent Docker volumes. `down` removes their stored test data.
- `.env` is ignored by Git. Never put production, staging, or live payment credentials in it.

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

Change `TEST_SQL_PASSWORD` to a strong local-only password. Keep `TEST_JWT_KEY` local and private as well. Then start the SQL Server container, apply the current EF migrations to the disposable test database, and launch the API:

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

The current API test project uses test doubles. The existing test suites remain independent of Docker.

## Stop and discard the environment

```powershell
docker compose --env-file tests/environment/.env -f tests/environment/docker-compose.yml down
```

This removes the test SQL container. Remove the temporary uploads directory and `tests/environment/.env` when finished if you no longer need them.
