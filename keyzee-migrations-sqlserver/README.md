# KeyZee SQL Server Migrations

This project contains Entity Framework Core migrations for KeyZee when using SQL Server.

## Building Migration Bundles

Migration bundles are self-contained executables that can run database migrations without requiring the .NET SDK.

### Build All Platform Bundles

```bash
# Linux/macOS
chmod +x build-bundles.sh
./build-bundles.sh

# Windows
.\build-bundles.ps1
```

### Build Individual Bundle

```bash
# Linux
dotnet ef migrations bundle --self-contained -r linux-x64 -o keyzee-bundle

# Windows
dotnet ef migrations bundle --self-contained -r win-x64 -o keyzee-bundle.exe

# macOS (Intel)
dotnet ef migrations bundle --self-contained -r osx-x64 -o keyzee-bundle

# macOS (Apple Silicon)
dotnet ef migrations bundle --self-contained -r osx-arm64 -o keyzee-bundle
```

## Using the Bundle

```bash
# Run the bundle with your connection string
./keyzee-sqlserver-linux-x64 --connection "Server=localhost;Database=KeyZee;User Id=sa;Password=YourPassword;"

# Or use environment variable
export ConnectionStrings__DefaultConnection="Server=localhost;Database=KeyZee;..."
./keyzee-sqlserver-linux-x64
```

## Adding New Migrations

When KeyZee entities change, add a new migration:

```bash
dotnet ef migrations add YourMigrationName
```

Then rebuild the bundles using the build scripts above.

## CI/CD Integration

Include the bundle building in your CI/CD pipeline:

```yaml
# GitHub Actions example
- name: Build Migration Bundles
  run: |
    cd keyzee-migrations-sqlserver
    dotnet ef migrations bundle --configuration Release --self-contained -r linux-x64 -o ../artifacts/keyzee-bundle-linux
    dotnet ef migrations bundle --configuration Release --self-contained -r win-x64 -o ../artifacts/keyzee-bundle-windows.exe
```