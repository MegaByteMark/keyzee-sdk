# Exit on error
$ErrorActionPreference = "Stop"

# Check if migration name is provided
if (-not $args[0]) {
    Write-Host "Usage: .\add-migrations.ps1 <MigrationName>"
    Write-Host "Example: .\add-migrations.ps1 AddUserTable"
    exit 1
}

$MigrationName = $args[0]

Write-Host "Adding migration '$MigrationName' to all providers..."

# Add migration to PostgreSQL project
Write-Host "Adding migration to PostgreSQL..."
dotnet ef migrations add $MigrationName --project ./keyzee-migrations-postgresql/keyzee-migrations-postgresql.csproj

# Add migration to SQLite project
Write-Host "Adding migration to SQLite..."
dotnet ef migrations add $MigrationName --project ./keyzee-migrations-sqlite/keyzee-migrations-sqlite.csproj

# Add migration to SQL Server project
Write-Host "Adding migration to SQL Server..."
dotnet ef migrations add $MigrationName --project ./keyzee-migrations-sqlserver/keyzee-migrations-sqlserver.csproj

Write-Host "Migrations added successfully!"