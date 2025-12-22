using System.ComponentModel.DataAnnotations;
using IntraDotNet.Domain.Core;

namespace KeyZee.Domain.Models;

/// <summary>
/// Represents an application within the KeyZee system.
/// </summary>
public sealed class App : IAuditable
{
    [Required]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(200)]
    public required string Name { get; set; }

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