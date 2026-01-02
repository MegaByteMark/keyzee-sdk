#!/bin/bash

# Exit on error
set -e

# Check if migration name is provided
if [ -z "$1" ]; then
    echo "Usage: ./add-migration-and-build.sh <MigrationName>"
    echo "Example: ./add-migration-and-build.sh AddUserTable"
    exit 1
fi

MIGRATION_NAME=$1

echo "Adding migration '$MIGRATION_NAME' to all providers..."

# Add migration to PostgreSQL project
echo "Adding migration to PostgreSQL..."
dotnet ef migrations add $MIGRATION_NAME --project ./keyzee-migrations-postgresql/keyzee-migrations-postgresql.csproj

# Add migration to SQLite project
echo "Adding migration to SQLite..."
dotnet ef migrations add $MIGRATION_NAME --project ./keyzee-migrations-sqlite/keyzee-migrations-sqlite.csproj

# Add migration to SQL Server project
echo "Adding migration to SQL Server..."
dotnet ef migrations add $MIGRATION_NAME --project ./keyzee-migrations-sqlserver/keyzee-migrations-sqlserver.csproj

echo "Migrations added successfully!"
echo ""
echo "Building migration bundles..."

# Run the build-bundles script
echo "Building migrations bundle for PostgreSQL..."
cd ./keyzee-migrations-postgresql
./build-bundles.sh
cd ..

echo "Building migrations bundle for SQLite..."
cd ./keyzee-migrations-sqlite
./build-bundles.sh
cd ..

echo "Building migrations bundle for SQL Server..."
cd ./keyzee-migrations-sqlserver
./build-bundles.sh
cd ..

echo "Done! Migrations added and bundles built successfully."