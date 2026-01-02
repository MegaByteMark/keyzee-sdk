Write-Host "Building KeyZee Postgresql Migration Bundles..." -ForegroundColor Green

# Create bundles directory
New-Item -ItemType Directory -Force -Path ".\bundles" | Out-Null

# Build for Linux
Write-Host "Building Linux bundle..." -ForegroundColor Yellow
dotnet ef migrations bundle --configuration Release --self-contained -r linux-x64 -o .\bundles\keyzee-postgresql-linux-x64

# Build for Windows
Write-Host "Building Windows bundle..." -ForegroundColor Yellow
dotnet ef migrations bundle --configuration Release --self-contained -r win-x64 -o .\bundles\keyzee-postgresql-win-x64.exe
# Build for macOS
Write-Host "Building macOS bundle..." -ForegroundColor Yellow
dotnet ef migrations bundle --configuration Release --self-contained -r osx-x64 -o .\bundles\keyzee-postgresql-osx-x64

# Build for macOS ARM
Write-Host "Building macOS ARM bundle..." -ForegroundColor Yellow
dotnet ef migrations bundle --configuration Release --self-contained -r osx-arm64 -o .\bundles\keyzee-postgresql-osx-arm64

Write-Host "All bundles created in .\bundles directory" -ForegroundColor Green