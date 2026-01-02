# Exit on error
$ErrorActionPreference = "Stop"

Write-Host "Building KeyZee SDK..."
dotnet clean
dotnet restore
dotnet build --configuration Release

Write-Host "Running tests..."
dotnet test --configuration Release

Write-Host "Build and tests completed successfully!"
Write-Host ""

Write-Host "Generating Migration Bundles for all providers..."

# Generate Migration Bundle for PostgreSQL
Write-Host "Building KeyZee Postgresql Migration Bundles..."

# Build for Linux
Write-Host "Building PostgreSQL Linux bundle..."
dotnet ef migrations bundle --configuration Release --project ./keyzee-migrations-postgresql/keyzee-migrations-postgresql.csproj --self-contained -r linux-x64 -o ./bundles/keyzee-postgresql-linux-x64

# Build for Windows
Write-Host "Building PostgreSQL Windows bundle..."
dotnet ef migrations bundle --configuration Release --project ./keyzee-migrations-postgresql/keyzee-migrations-postgresql.csproj --self-contained -r win-x64 -o ./bundles/keyzee-postgresql-win-x64.exe

# Build for macOS
Write-Host "Building PostgreSQL macOS bundle..."
dotnet ef migrations bundle --configuration Release --project ./keyzee-migrations-postgresql/keyzee-migrations-postgresql.csproj --self-contained -r osx-x64 -o ./bundles/keyzee-postgresql-osx-x64

# Build for macOS ARM (Apple Silicon)
Write-Host "Building PostgreSQL macOS ARM bundle..."
dotnet ef migrations bundle --configuration Release --project ./keyzee-migrations-postgresql/keyzee-migrations-postgresql.csproj --self-contained -r osx-arm64 -o ./bundles/keyzee-postgresql-osx-arm64

Write-Host "All PostgreSQL bundles created in ./bundles directory"

# Generate Migration Bundle for SQLite
Write-Host "Building KeyZee SQLite Migration Bundles..."

# Build for Linux
Write-Host "Building SQLite Linux bundle..."
dotnet ef migrations bundle --configuration Release --project ./keyzee-migrations-sqlite/keyzee-migrations-sqlite.csproj --self-contained -r linux-x64 -o ./bundles/keyzee-sqlite-linux-x64

# Build for Windows
Write-Host "Building SQLite Windows bundle..."
dotnet ef migrations bundle --configuration Release --project ./keyzee-migrations-sqlite/keyzee-migrations-sqlite.csproj --self-contained -r win-x64 -o ./bundles/keyzee-sqlite-win-x64.exe

# Build for macOS
Write-Host "Building SQLite macOS bundle..."
dotnet ef migrations bundle --configuration Release --project ./keyzee-migrations-sqlite/keyzee-migrations-sqlite.csproj --self-contained -r osx-x64 -o ./bundles/keyzee-sqlite-osx-x64

# Build for macOS ARM (Apple Silicon)
Write-Host "Building SQLite macOS ARM bundle..."
dotnet ef migrations bundle --configuration Release --project ./keyzee-migrations-sqlite/keyzee-migrations-sqlite.csproj --self-contained -r osx-arm64 -o ./bundles/keyzee-sqlite-osx-arm64

Write-Host "All SQLite bundles created in ./bundles directory"

# Generate Migration Bundle for SQL Server
Write-Host "Building KeyZee SQL Server Migration Bundles..."

# Build for Linux
Write-Host "Building SQL Server Linux bundle..."
dotnet ef migrations bundle --configuration Release --project ./keyzee-migrations-sqlserver/keyzee-migrations-sqlserver.csproj --self-contained -r linux-x64 -o ./bundles/keyzee-sqlserver-linux-x64

# Build for Windows
Write-Host "Building SQL Server Windows bundle..."
dotnet ef migrations bundle --configuration Release --project ./keyzee-migrations-sqlserver/keyzee-migrations-sqlserver.csproj --self-contained -r win-x64 -o ./bundles/keyzee-sqlserver-win-x64.exe

# Build for macOS
Write-Host "Building SQL Server macOS bundle..."
dotnet ef migrations bundle --configuration Release --project ./keyzee-migrations-sqlserver/keyzee-migrations-sqlserver.csproj --self-contained -r osx-x64 -o ./bundles/keyzee-sqlserver-osx-x64

# Build for macOS ARM (Apple Silicon)
Write-Host "Building SQL Server macOS ARM bundle..."
dotnet ef migrations bundle --configuration Release --project ./keyzee-migrations-sqlserver/keyzee-migrations-sqlserver.csproj --self-contained -r osx-arm64 -o ./bundles/keyzee-sqlserver-osx-arm64

Write-Host "All SQL Server bundles created in ./bundles directory"
Write-Host ""

Write-Host "Migration Bundles generation completed successfully!"