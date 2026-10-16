[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$environmentDir = $PSScriptRoot
$envFile = Join-Path $environmentDir '.env'
$composeFile = Join-Path $environmentDir 'docker-compose.yml'

if (-not (Get-Command docker -ErrorAction SilentlyContinue)) {
    throw 'Docker CLI was not found. Install/start Docker Desktop, then run this script again.'
}

if (-not (Test-Path $envFile)) {
    Copy-Item (Join-Path $environmentDir '.env.example') $envFile
    throw 'Created tests\environment\.env. Set a private local TEST_SQL_PASSWORD in that file, then rerun this script.'
}

$settings = @{}
foreach ($line in Get-Content $envFile) {
    if ($line -match '^\s*([^#=\s]+)\s*=\s*(.*?)\s*$') {
        $settings[$Matches[1]] = $Matches[2].Trim('"').Trim("'")
    }
}

$sqlPassword = $settings['TEST_SQL_PASSWORD']
$jwtKey = $settings['TEST_JWT_KEY']
if ([string]::IsNullOrWhiteSpace($sqlPassword) -or $sqlPassword -match 'ChangeMe') {
    throw 'Set TEST_SQL_PASSWORD to a strong local-only password in tests\environment\.env.'
}
if ([string]::IsNullOrWhiteSpace($jwtKey) -or $jwtKey.Length -lt 32) {
    throw 'Set TEST_JWT_KEY to a random local-only value of at least 32 characters in tests\environment\.env.'
}

Push-Location $repoRoot
try {
    & docker compose --env-file $envFile -f $composeFile up -d
    if ($LASTEXITCODE -ne 0) { throw 'Could not start isolated SQL Server and Azurite containers.' }

    $deadline = (Get-Date).AddSeconds(120)
    $connected = $false
    while ((Get-Date) -lt $deadline -and -not $connected) {
        $client = [System.Net.Sockets.TcpClient]::new()
        try {
            $client.Connect('127.0.0.1', 14330)
            $connected = $true
        }
        catch {
            Start-Sleep -Seconds 2
        }
        finally {
            $client.Dispose()
        }
    }
    if (-not $connected) { throw 'Test SQL Server did not become reachable at 127.0.0.1:14330 within 120 seconds.' }

    $env:ASPNETCORE_ENVIRONMENT = 'Development'
    $env:ConnectionStrings__DefaultConnection = "Server=127.0.0.1,14330;Database=RashePharma_AutomatedTests;User Id=sa;Password=$sqlPassword;Encrypt=False;TrustServerCertificate=True;Connect Timeout=30"
    $env:ConnectionStrings__AzureStorage = 'UseDevelopmentStorage=true'
    $env:JWT__KEY = $jwtKey
    $env:Stripe__SecretKey = 'sk_test_local_placeholder_not_a_live_key'

    & dotnet ef database update --project src/RashePharma.Infrastructure --startup-project src/RashePharma.Api --context ApplicationDbContext
    if ($LASTEXITCODE -ne 0) { throw 'Applying the application migrations to the disposable test database failed.' }

    Write-Host 'Starting the API against the local disposable test database and Azurite. Press Ctrl+C to stop.'
    & dotnet run --project src/RashePharma.Api --no-launch-profile --urls http://localhost:5104
    if ($LASTEXITCODE -ne 0) { throw 'The local API process exited with an error.' }
}
finally {
    Pop-Location
}
