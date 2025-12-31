#!/bin/bash

echo "Building KeyZee SQL Server Migration Bundles..."

# Build for Linux
echo "Building Linux bundle..."
dotnet ef migrations bundle --configuration Release --self-contained -r linux-x64 -o ./bundles/keyzee-sqlserver-linux-x64

# Build for Windows
echo "Building Windows bundle..."
dotnet ef migrations bundle --configuration Release --self-contained -r win-x64 -o ./bundles/keyzee-sqlserver-win-x64.exe

# Build for macOS
echo "Building macOS bundle..."
dotnet ef migrations bundle --configuration Release --self-contained -r osx-x64 -o ./bundles/keyzee-sqlserver-osx-x64

# Build for macOS ARM (Apple Silicon)
echo "Building macOS ARM bundle..."
dotnet ef migrations bundle --configuration Release --self-contained -r osx-arm64 -o ./bundles/keyzee-sqlserver-osx-arm64

echo "All bundles created in ./bundles directory"