# KeyZee - Minimal Self-hostable Encrypted Key-Value-Pair Store

[![.NET](https://img.shields.io/badge/.NET-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white)](https://docs.microsoft.com/en-us/dotnet/csharp/)
[![License](https://img.shields.io/badge/License-MIT-blue.svg?style=for-the-badge)](LICENSE)

## Table Of Contents
- [Overview](#overview)
- [Project Structure](#project-structure)
- [Getting Started](#getting-started)
- [Complete Usage Example](#complete-usage-example)
- [See Also](#see-also)

## Overview

KeyZee is a lightweight, self-hostable encrypted key-value pair store built with .NET. It provides a secure way to store sensitive configuration data, secrets, and key-value pairs with built-in AES encryption. KeyZee is designed following Clean Architecture principles, making it maintainable, testable, and easily extensible.

### Key Features

- **🔐 Built-in Encryption**: All values are encrypted using AES-256 encryption before storage
- **🏢 Multi-Application Support**: Organise key-value pairs by application for better isolation and management
- **🗄️ Database Agnostic**: Works with any relational database supported by Entity Framework Core (SQL Server, PostgreSQL, SQLite, etc.)
- **♻️ Soft Delete**: Implements soft delete pattern for data recovery and audit trails
- **🔄 Migration Tools**: Built-in support for re-encrypting existing data with new encryption keys
- **🎯 Clean Architecture**: Follows Clean Architecture principles with clear separation of concerns
- **📦 Dependency Injection Ready**: Designed for easy integration into ASP.NET Core and console applications
- **⚡ Async/Await**: Fully asynchronous API for optimal performance
- **✅ Result Pattern**: Uses Result pattern for predictable error handling without exceptions
- **🧪 Testable**: Highly testable design with interfaces and dependency injection

## Project Structure

KeyZee follows Clean Architecture with three main layers:

```
KeyZee/
├── KeyZee.Domain/              # Core business entities and value objects
│   ├── Models/
│   │   ├── App.cs             # Application entity
│   │   └── KeyValuePair.cs    # Key-value pair entity
│
├── KeyZee.Application/         # Business logic and use cases
│   ├── Common/
│   │   ├── Encryption/
│   │   │   ├── IEncryptionService.cs
|   |   ├── Persistence/
|   |   |   ├── IAppRepository.cs
|   |   |   ├── IKeyValuePairRepository.cs
|   |   |   ├── IKeyZeeUnitOfWork.cs
│   │   ├── Services/
│   │   │   ├── IAppService.cs
│   │   │   ├── IKeyValuePairService.cs
│   ├── Services/
│   │   ├── AppService.cs       # Business logic for applications
|   |   ├── KeyValuePair.cs       # Business logic for key value pairs
│   ├── Validation/
│   │   ├── AppValidator.cs                # Validation logic for applications
|   |   ├── KeyValuePairValidator.cs       # Validation logic for key value pairs
|   
├── KeyZee.DependencyInjection/
│   |   └── DependencyInjectionExtensions.cs  # Registrations for Dependency Injection
|
└── KeyZee.Infrastructure/      # Data access and external concerns
    ├── DbContext/
    │   ├── KeyZeeDbContext.cs
    ├── Encryption/
    │   ├── AesEncryptionService.cs
    ├── Options/
    │   ├── KeyZeeOptions.cs
    │   ├── KeyZeeOptionsBuilder.cs
    ├── Repositories/
    │   ├── AppRepository.cs
    │   ├── KeyValuePairRepository.cs
    └── UnitOfWork/
    │   └──KeyZeeUnitOfWork.cs
```

### Layer Responsibilities

- **Domain**: Contains core business entities (`App`, `KeyValuePair`) with no external dependencies
- **Application**: Contains business logic, services, and interfaces for repositories and encryption
- **Infrastructure**: Implements data access using Entity Framework Core and handles database operations

## Getting Started

### Installation

Install the KeyZee SDK via NuGet:

```bash
dotnet add package KeyZee
```

Or add to your `.csproj` file:

```xml
<PackageReference Include="KeyZee" Version="1.0.0" />
```

### Basic Setup

#### 1. Configure Services in ASP.NET Core
**IMPORTANT**: It is strongly recommended to keep sensitive information out of configuration files and ensure they are not commited to version control. 

One way to achieve this is to use `.env` files for local development with packages like [dotenv.net](https://github.com/bolorundurowb/dotenv.net) and platform based environment variables in Production scenarios. 

```csharp
using KeyZee.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using dotenv.net;

var builder = WebApplication.CreateBuilder(args);

//Load Environment Variables from .env file for local development, make sure .env is not committed to version control.
DotEnv.Load();

// Add KeyZee services
builder.Services.AddKeyZee(options =>
{
    options
        .WithAppName("MyApp")
        .WithEncryptionKey(Environment.GetEnvironmentVariable("ENCRYPTION_KEY"))
        .WithEncryptionSecret(Environment.GetEnvironmentVariable("ENCRYPTION_SECRET"))
        .WithDbContextOptions(dbOptions =>
        {
            //This example uses Microsoft Sql Server, other providers are available...
            dbOptions.UseSqlServer(Environment.GetEnvironmentVariable("DB_CONN_STRING"));
        });
});

var app = builder.Build();
app.Run();
```

#### 2. Configure Encryption Keys

Generate encryption keys as plain text strings (32 characters for key, 16 characters for secret):

```csharp
// Example: Generate random keys (do this once, then store securely)
// Key must be exactly 32 characters
var key = "abcdefghijklmnopqrstuvwxyz123456";

// Secret/IV must be exactly 16 characters  
var secret = "abcdefghijklmnop";

// Or generate random alphanumeric strings:
var key = GenerateRandomString(32);
var secret = GenerateRandomString(16);

string GenerateRandomString(int length)
{
    const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
    var random = new Random();
    return new string(Enumerable.Repeat(chars, length)
        .Select(s => s[random.Next(s.Length)]).ToArray());
}
```

**Important:** The encryption key must be exactly 32 characters and the secret must be exactly 16 characters for AES-256 encryption.

Add to your environment variables or `.env` file:

```env
ENCRYPTION_KEY=abcdefghijklmnopqrstuvwxyz123456
ENCRYPTION_SECRET=abcdefghijklmnop
DB_CONN_STRING=Server=localhost;Database=KeyZeeStore;Trusted_Connection=true;
```

#### 3. Database Setup

KeyZee uses Entity Framework Core for data access. You have several options for setting up the database:

**Option A: Use EF Core Migration Bundle (Recommended for Production)**

KeyZee provides pre-built migration bundles for common database providers. Download and run the appropriate bundle for your database:

```bash
# Download the migration bundle for your database provider
# SQL Server
wget https://github.com/MegaByteMark/keyzee-sdk/releases/download/v1.0.0/keyzee-sqlserver-osx-arm64

# Make it executable (Linux/Mac)
chmod +x keyzee-sqlserver-osx-arm64

# Run migrations
./keyzee-sqlserver-osx-arm64 --connection "Server=localhost;Database=KeyZeeStore;..."
```

Or build your own bundle from the KeyZee.Migrations project:
**Note**: This step assumes you have already downloaded the KeyZee-SDK project from GitHub.

```bash
# Navigate to the migrations project
cd keyzee-migrations-sqlserver

# Create a migration bundle
dotnet ef migrations bundle --configuration Release

# Run the bundle
./efbundle --connection "Server=localhost;Database=KeyZeeStore;..."
```

**Option B: Automatic Migration (Simplest for Development)**

Add this code to automatically apply migrations on application startup:

```csharp
using KeyZee.Infrastructure.DbContext;

var app = builder.Build();

// Automatically apply migrations on startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<KeyZeeDbContext>();
    await dbContext.Database.MigrateAsync();
}

app.Run();
```

⚠️ **Warning**: Automatic migrations are convenient for development but should be carefully reviewed for production use.

**Option C: Manual Migrations (Full Control)**

Create and manage migrations in your own project:

```bash
# Install EF Core tools if you haven't already
dotnet tool install --global dotnet-ef

# Add the KeyZee DbContext to your project, then create a migration
dotnet ef migrations add AddKeyZee --context KeyZeeDbContext

# Apply migrations
dotnet ef database update --context KeyZeeDbContext

# Or generate SQL scripts for DBA review
dotnet ef migrations script --context KeyZeeDbContext --output keyzee-migration.sql
```

**Creating Your Own Migration Bundle**

If you want to create a migration bundle with your own customisations:

```bash
# In your consuming application
dotnet ef migrations bundle --context KeyZeeDbContext --output keyzee-bundle

# This creates a self-contained executable with all migrations
# Run it with your connection string:
./keyzee-bundle --connection "your-connection-string"
```

Migration bundles are ideal for:
- Production deployments where you need isolated migration execution
- CI/CD pipelines
- Environments where you can't install .NET SDK
- DBA-controlled database updates

#### 4. Use KeyZee Services

Inject and use the services in your application:

```csharp
public class SecretsController : ControllerBase
{
    private readonly IKeyValuePairService _kvpService;
    private readonly IEncryptionService _encryptionService;

    public SecretsController(
        IKeyValuePairService kvpService,
        IEncryptionService encryptionService)
    {
        _kvpService = kvpService;
        _encryptionService = encryptionService;
    }

    [HttpGet("{key}")]
    public async Task<IActionResult> GetSecret(string key)
    {
        var result = await _kvpService.GetKeyValuePairByAppAndKeyAsync("MyApp", key);
        
        if (result.IsFailure)
            return BadRequest(result.AggregateErrors);
            
        if (result.Value == null)
            return NotFound();
            
        var decryptedValue = _encryptionService.Decrypt(result.Value.EncryptedValue);
        return Ok(new { key, value = decryptedValue });
    }

    [HttpPost]
    public async Task<IActionResult> SetSecret([FromBody] SecretRequest request)
    {
        var encryptedValue = _encryptionService.Encrypt(request.Value);
        
        var kvp = new KeyValuePair
        {
            Key = request.Key,
            EncryptedValue = encryptedValue,
            AppId = 1 // Get from app service
        };
        
        var result = await _kvpService.CreateAsync(kvp);
        
        if (result.IsFailure)
            return BadRequest(result.AggregateErrors);
            
        return Ok();
    }
}
```

## Complete Usage Example

### Creating a Console Application with KeyZee

```csharp
using KeyZee.Application.Common.Services;
using KeyZee.Application.Common.Encryption;
using KeyZee.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

// Setup dependency injection
var services = new ServiceCollection();

services.AddKeyZee(options =>
{
    options
        .WithAppName("MyConsoleApp")
        .WithEncryptionKey(Environment.GetEnvironmentVariable("KEYZEE_ENCRYPTION_KEY"))
        .WithEncryptionSecret(Environment.GetEnvironmentVariable("KEYZEE_ENCRYPTION_SECRET"))
        .WithDbContextOptions(builder =>
        {
            builder.UseSqlServer(Environment.GetEnvironmentVariable("KEYZEE_CONNECTION_STRING"));
        });
});

var serviceProvider = services.BuildServiceProvider();

// Get services
var appService = serviceProvider.GetRequiredService<IAppService>();
var kvpService = serviceProvider.GetRequiredService<IKeyValuePairService>();
var encryptionService = serviceProvider.GetRequiredService<IEncryptionService>();

// Create an application
var appResult = await appService.GetByNameAsync("MyConsoleApp");

if (appResult.Value == null)
{
    var app = new KeyZee.Domain.Models.App { Name = "MyConsoleApp" };
    await appService.CreateAsync(app);
    Console.WriteLine("App created!");
}

// Store an encrypted value
var encryptedValue = encryptionService.Encrypt("SuperSecretPassword123!");

var kvp = new KeyZee.Domain.Models.KeyValuePair
{
    AppId = appResult.Value!.Id,
    Key = "DatabasePassword",
    EncryptedValue = encryptedValue
};

var createResult = await kvpService.CreateAsync(kvp);

if (createResult.IsSuccess)
{
    Console.WriteLine("Secret stored successfully!");
}

// Retrieve and decrypt a value
var getResult = await kvpService.GetKeyValuePairByAppAndKeyAsync("MyConsoleApp", "DatabasePassword");

if (getResult.IsSuccess && getResult.Value != null)
{
    var decryptedValue = encryptionService.Decrypt(getResult.Value.EncryptedValue);
    Console.WriteLine($"Retrieved secret: {decryptedValue}");
}

// List all keys for an app
var listResult = await kvpService.GetKeyValuePairsByAppAsync("MyConsoleApp");

if (listResult.IsSuccess)
{
    Console.WriteLine("All keys:");
    foreach (var k in listResult.Value!)
    {
        Console.WriteLine($"  - {k.Key}");
    }
}

// Migrate to new encryption keys (re-encrypt all data)
var migrateResult = await kvpService.MigrateByAppAsync(
    "MyConsoleApp",
    "new-encryption-key-32chars",
    "new-encryption-secret-16chars"
);

if (migrateResult.IsSuccess)
{
    Console.WriteLine("All data re-encrypted successfully!");
}

// Delete a key-value pair
var deleteResult = await kvpService.DeleteKeyValuePairByAppAndKeyAsync("MyConsoleApp", "DatabasePassword");

if (deleteResult.IsSuccess)
{
    Console.WriteLine("Secret deleted!");
}
```

### Error Handling with Result Pattern

```csharp
var result = await kvpService.GetKeyValuePairByAppAndKeyAsync("MyApp", "ApiKey");

if (result.IsFailure)
{
    // Handle errors
    Console.WriteLine($"Error occurred: {result.AggregateErrors}");
    return;
}

if (result.Value == null)
{
    Console.WriteLine("Key not found");
    return;
}

// Use the value
var decryptedValue = encryptionService.Decrypt(result.Value.EncryptedValue);
Console.WriteLine($"API Key: {decryptedValue}");
```

## See Also
- [KeyZee CLI Tool](https://github.com/MegaByteMark/keyzee-cli) - Command-line interface for KeyZee
- [Clean Architecture Principles](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
