namespace KeyZee.Application.Dtos;

/// <summary>
/// Data Transfer Object for KeyValuePair entity.
/// </summary>
public sealed class KeyValuePairDto
{
    /// <summary>
    /// Gets or sets the name of the App associated with the KeyValuePair.
    /// </summary>
    public required string AppName { get; set; }
    /// <summary>
    /// Gets or sets the key of the KeyValuePair.
    /// </summary>
    public required string Key { get; set; }
    /// <summary>
    /// Gets or sets the value of the KeyValuePair.
    /// </summary>
    public required string Value { get; set; }
}
