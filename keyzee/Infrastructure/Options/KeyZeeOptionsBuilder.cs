using Microsoft.EntityFrameworkCore;

namespace KeyZee.Infrastructure.Options;

public sealed class KeyZeeOptionsBuilder
{
    private readonly KeyZeeOptions _options = new();

    public KeyZeeOptionsBuilder()
    {
    }

    public KeyZeeOptionsBuilder WithDbContextOptions(Action<DbContextOptionsBuilder> dbContextOptions)
    {
        _options.OptionsBuilder = dbContextOptions;

        return this;
    }

    public KeyZeeOptionsBuilder WithEncryptionKey(string encryptionKey)
    {
        _options.EncryptionKey = encryptionKey;

        return this;
    }

    public KeyZeeOptionsBuilder WithEncryptionSecret(string encryptionSecret)
    {
        _options.EncryptionSecret = encryptionSecret;

        return this;
    }

    public KeyZeeOptionsBuilder WithAppName(string applicationName)
    {
        _options.AppName = applicationName;

        return this;
    }

    internal KeyZeeOptions Build()
    {
        return _options;
    }
}