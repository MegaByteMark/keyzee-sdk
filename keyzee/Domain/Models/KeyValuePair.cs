using System.ComponentModel.DataAnnotations;
using IntraDotNet.Domain.Core;

namespace KeyZee.Domain.Models;

/// <summary>
/// Represents a key-value pair within an application in the KeyZee system.
/// </summary>
public sealed class KeyValuePair: IAuditable
{
    [Required]
    public Guid Id { get; set; }

    [Required]
    public required Guid AppId { get; set; }

    [Required]
    [MaxLength(200)]
    public required string Key { get; set; }

    [Required]
    public required string EncryptedValue { get; set; }

    public App? Application { get; set; }

    public DateTimeOffset? CreatedOn { get; set; }

    [MaxLength(200)]
    public string? CreatedBy { get; set; }
    public DateTimeOffset? LastUpdateOn { get; set; }

    [MaxLength(200)]
    public string? LastUpdateBy { get; set; }
    public DateTimeOffset? DeletedOn { get; set; }

    [MaxLength(200)]
    public string? DeletedBy { get; set; }
}
