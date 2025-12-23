using IntraDotNet.CleanArchitecture.Domain.Common.Persistence;

namespace KeyZee.Application.Dtos;

/// <summary>
/// Data Transfer Object for App entity.
/// </summary>
public sealed class AppDto: IGuidIdentifier
{
    /// <summary>
    /// Gets or sets the unique identifier for the App.
    /// </summary>
    public Guid Id { get; set; }
    /// <summary>
    /// Gets or sets the name of the App.
    /// </summary>
    public required string Name { get; set; }
}
