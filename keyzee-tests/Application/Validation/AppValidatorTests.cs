using KeyZee.Application.Validation;

namespace KeyZee.Tests.Application.Validation;

public class AppValidatorTests
{
    private readonly AppValidator _validator;

    public AppValidatorTests()
    {
        _validator = new AppValidator();
    }

    [Fact]
    public void Validate_ValidApp_NoValidationErrors()
    {
        // Arrange
        var App = new Domain.Models.App
        {
            Name = "ValidAppName",
            Id = Guid.NewGuid()
        };

        // Act
        var result = _validator.Validate(App);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_InvalidApp_ReturnsValidationErrorsIfNameIsBlank()
    {
        // Arrange
        var App = new Domain.Models.App
        {
            Name = "", // Invalid: Name is required
            Id = Guid.NewGuid()
        };

        // Act
        var result = _validator.Validate(App);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name");
    }

    [Fact]
    public void Validate_InvalidApp_ReturnsValidationErrorsIfNameTooLong()
    {
        // Arrange
        var App = new Domain.Models.App
        {
            Name = new string('A', 256), // Invalid: Name exceeds max length
            Id = Guid.NewGuid()
        };

        // Act
        var result = _validator.Validate(App);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name");
    }

    [Fact]
    public void Validate_InvalidApp_ReturnsValidationErrorsIfNameHasInvalidCharacters()
    {
        // Arrange
        var App = new Domain.Models.App
        {
            Name = "Invalid@Name!", // Invalid: Name contains special characters
            Id = Guid.NewGuid()
        };

        // Act
        var result = _validator.Validate(App);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name");
    }

    [Fact]
    public void Validate_InvalidApp_ReturnsMultipleValidationErrors()
    {
        // Arrange
        var App = new Domain.Models.App
        {
            Name = new string('@', 256), // Invalid: Too long and has invalid characters
            Id = Guid.NewGuid()
        };

        // Act
        var result = _validator.Validate(App);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal(2, result.Errors.Count); // Expecting two errors: Length and Matches
    }
}
