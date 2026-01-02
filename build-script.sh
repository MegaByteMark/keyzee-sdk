#!/bin/bash

# Exit on error
set -e

echo "Building KeyZee SDK..."
dotnet clean
dotnet restore
dotnet build --configuration Release

echo "Running tests..."
dotnet test --configuration Release

echo "Build and tests completed successfully!"
echo ""

echo "Generating Migration Bundles for all providers..."

# Generate Migration Bundle for PostgreSQL
echo "Building KeyZee Postgresql Migration Bundles..."

# Build for Linux
echo "Building PostgreSQL Linux bundle..."
dotnet ef migrations bundle --configuration Release --project ./keyzee-migrations-postgresql/keyzee-migrations-postgresql.csproj --self-contained -r linux-x64 -o ./bundles/keyzee-postgresql-linux-x64

# Build for Windows
echo "Building PostgreSQL Windows bundle..."
dotnet ef migrations bundle --configuration Release --project ./keyzee-migrations-postgresql/keyzee-migrations-postgresql.csproj --self-contained -r win-x64 -o ./bundles/keyzee-postgresql-win-x64.exe
# Build for macOS
echo "Building PostgreSQL macOS bundle..."
dotnet ef migrations bundle --configuration Release --project ./keyzee-migrations-postgresql/keyzee-migrations-postgresql.csproj --self-contained -r osx-x64 -o ./bundles/keyzee-postgresql-osx-x64

# Build for macOS ARM (Apple Silicon)
echo "Building PostgreSQL macOS ARM bundle..."
dotnet ef migrations bundle --configuration Release --project ./keyzee-migrations-postgresql/keyzee-migrations-postgresql.csproj --self-contained -r osx-arm64 -o ./bundles/keyzee-postgresql-osx-arm64

echo "All PostgreSQL bundles created in ./bundles directory"

# Generate Migration Bundle for SQLite
echo "Building KeyZee SQLite Migration Bundles..."

# Build for Linux
echo "Building SQLite Linux bundle..."
dotnet ef migrations bundle --configuration Release --project ./keyzee-migrations-sqlite/keyzee-migrations-sqlite.csproj --self-contained -r linux-x64 -o ./bundles/keyzee-sqlite-linux-x64

# Build for Windows
echo "Building SQLite Windows bundle..."
dotnet ef migrations bundle --configuration Release --project ./keyzee-migrations-sqlite/keyzee-migrations-sqlite.csproj --self-contained -r win-x64 -o ./bundles/keyzee-sqlite-win-x64.exe

# Build for macOS
echo "Building SQLite macOS bundle..."
dotnet ef migrations bundle --configuration Release --project ./keyzee-migrations-sqlite/keyzee-migrations-sqlite.csproj --self-contained -r osx-x64 -o ./bundles/keyzee-sqlite-osx-x64

# Build for macOS ARM (Apple Silicon)
echo "Building SQLite macOS ARM bundle..."
dotnet ef migrations bundle --configuration Release --project ./keyzee-migrations-sqlite/keyzee-migrations-sqlite.csproj --self-contained -r osx-arm64 -o ./bundles/keyzee-sqlite-osx-arm64

echo "All SQLite bundles created in ./bundles directory"

# Generate Migration Bundle for SQL Server
echo "Building KeyZee SQL Server Migration Bundles..."

# Build for Linux
echo "Building SQL Server Linux bundle..."
dotnet ef migrations bundle --configuration Release --project ./keyzee-migrations-sqlserver/keyzee-migrations-sqlserver.csproj --self-contained -r linux-x64 -o ./bundles/keyzee-sqlserver-linux-x64

# Build for Windows
echo "Building SQL Server Windows bundle..."
dotnet ef migrations bundle --configuration Release --project ./keyzee-migrations-sqlserver/keyzee-migrations-sqlserver.csproj --self-contained -r win-x64 -o ./bundles/keyzee-sqlserver-win-x64.exe

# Build for macOS
echo "Building SQL Server macOS bundle..."
dotnet ef migrations bundle --configuration Release --project ./keyzee-migrations-sqlserver/keyzee-migrations-sqlserver.csproj --self-contained -r osx-x64 -o ./bundles/keyzee-sqlserver-osx-x64

# Build for macOS ARM (Apple Silicon)
echo "Building SQL Server macOS ARM bundle..."
dotnet ef migrations bundle --configuration Release --project ./keyzee-migrations-sqlserver/keyzee-migrations-sqlserver.csproj --self-contained -r osx-arm64 -o ./bundles/keyzee-sqlserver-osx-arm64

echo "All SQL Server bundles created in ./bundles directory"
echo ""

echo "Migration Bundles generation completed successfully!"