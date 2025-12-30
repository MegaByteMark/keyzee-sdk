
using Microsoft.EntityFrameworkCore;

namespace KeyZee.Infrastructure.Options;

public sealed class KeyZeeOptionsBuilder
{
    private string _appName;
    private string _encryptionKey;
    private string _encryptionSecret;
    private Action<DbContextOptionsBuilder>? _dbContextOptionsBuilder;

    public KeyZeeOptionsBuilder()
    {
        _dbContextOptionsBuilder = null;
        _encryptionKey = string.Empty;
        _encryptionSecret = string.Empty;
        _appName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name ?? "Undefined";
    }

    public KeyZeeOptionsBuilder WithDbContextOptions(Action<DbContextOptionsBuilder> dbContextOptions)
    {
        _dbContextOptionsBuilder = dbContextOptions;

        return this;
    }

    public KeyZeeOptionsBuilder WithEncryptionKey(string encryptionKey)
    {
        _encryptionKey = encryptionKey;

        return this;
    }

    public KeyZeeOptionsBuilder WithEncryptionSecret(string encryptionSecret)
    {
        _encryptionSecret = encryptionSecret;

        return this;
    }

    public KeyZeeOptionsBuilder WithAppName(string appName)
    {
        _appName = appName;

        return this;
    }

    public KeyZeeOptions Build()
    {
        if(string.IsNullOrWhiteSpace(_encryptionKey))
        {
            throw new InvalidOperationException("Encryption key must be provided.");
        }

        if(string.IsNullOrWhiteSpace(_encryptionSecret))
        {
            throw new InvalidOperationException("Encryption secret must be provided.");
        }

        if(_dbContextOptionsBuilder == null)
        {
            throw new InvalidOperationException("DbContextOptions must be provided.");
        }

        return new KeyZeeOptions(_dbContextOptionsBuilder, _encryptionKey, _encryptionSecret, _appName);
    }
}