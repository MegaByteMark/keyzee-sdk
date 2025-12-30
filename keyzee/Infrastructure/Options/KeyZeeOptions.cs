using System.Text.Json.Serialization;
using System.Xml.Serialization;
using Microsoft.EntityFrameworkCore;

namespace KeyZee.Infrastructure.Options;

public sealed class KeyZeeOptions
{
    [JsonIgnore]
    public Action<DbContextOptionsBuilder> DbContextOptionsBuilder { get; set; }
    //Protect aginst accidental serialization
    [JsonIgnore]
    [XmlIgnore]
    public string EncryptionKey { get; set; }
    //Protect aginst accidental serialization
    [JsonIgnore]
    [XmlIgnore]
    public string EncryptionSecret { get; set; }
    public string AppName { get; set; }

    /// <summary>
    /// Default constructor for KeyZeeOptions, the executing assembly name is used as the AppName, the encryption key and secret are not set and need to be initialized later.
    /// </summary>
    public KeyZeeOptions()
    {
        DbContextOptionsBuilder = _ => { };
        EncryptionKey = string.Empty;
        EncryptionSecret = string.Empty;
        AppName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name ?? "Undefined";
    }

    /// <summary>
    /// Constructor for KeyZeeOptions
    /// </summary>
    /// <param name="options">The options builder action for DbContextOptionsBuilder.</param>
    /// <param name="encryptionKey">The encryption key used for securing data.</param>
    /// <param name="encryptionSecret">The encryption secret used for securing data.</param>
    /// <param name="appName">The name of the application.</param>
    public KeyZeeOptions(Action<DbContextOptionsBuilder> options, string encryptionKey, string encryptionSecret, string appName)
    {
        DbContextOptionsBuilder = options;
        EncryptionKey = encryptionKey;
        EncryptionSecret = encryptionSecret;
        AppName = appName;
    }

    //Protect aginst accidental logging
    public override string ToString()
    {
        return $"KeyZeeOptions {{ AppName = {AppName}, EncryptionKey = [REDACTED], EncryptionSecret = [REDACTED] }}";
    }
}