# Database migration release process

## Runtime rule

The API and BulkUploadWorker must not apply EF Core migrations during process startup.
Schema changes are applied once as an explicit release step by `RashePharma.DbMigrator`.

## Required configuration

Set the target database connection string in the process environment:

- `ConnectionStrings__DefaultConnection`

For a first-time environment bootstrap, also set:

- `ADMIN_EMAIL`
- `ADMIN_PASSWORD`

Do not commit connection strings or credentials to source control. Use the deployment platform's secret/environment-variable settings.

## Release sequence

1. Take/verify a database backup and confirm the target environment.
2. Build and publish the migration runner.
3. Run the migration runner once against the intended database.
4. For a new environment only, pass `--seed` to create the baseline roles and initial admin account. The role-name migration checks for duplicate or over-50-character role names before creating the unique index; resolve any reported data issue and retry.
5. Deploy/restart the API.
6. Deploy/restart the BulkUploadWorker.
7. Verify API health and critical database-backed flows.

Run migrations before deploying application code that requires the new schema. Do not run the migration runner concurrently from multiple deployment jobs.

## Local commands (Windows CMD)

Set the connection string in the current CMD session without printing it:

```cmd
set "ConnectionStrings__DefaultConnection=YOUR_LOCAL_SQL_CONNECTION_STRING"
```

Apply pending migrations:

```cmd
dotnet run --project src\RashePharma.DbMigrator\RashePharma.DbMigrator.csproj
```

Bootstrap roles and the initial admin in a new database:

```cmd
set "ADMIN_EMAIL=your-admin-email"
set "ADMIN_PASSWORD=your-admin-password"
dotnet run --project src\RashePharma.DbMigrator\RashePharma.DbMigrator.csproj -- --seed
```

The `--seed` option is explicit; unknown arguments are rejected. Normal migration runs do not require admin credentials and do not modify user accounts. When `--seed` is used, `ADMIN_EMAIL` identifies the bootstrap account. `ADMIN_PASSWORD` is required only if that account does not already exist. An existing account is left unchanged when it already has the Admin role; if the email belongs to a non-admin user, seeding fails without silently changing that user's role. The `Users.Email` unique index and `Roles.Name` unique index provide database-level duplicate protection.

## CI and deployment

CI may continue applying migrations to its disposable integration-test database before running SQL Server integration tests. Production/staging deployment must invoke the migrator as a separate, single-run release step; API and worker containers only start application processes.

## Rollback

Do not automatically run `database update <previous-migration>` as an application rollback. Prefer a forward corrective migration. Restore from a verified backup only under the approved database recovery procedure.
