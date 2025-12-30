using KeyZee.Application.Dtos;
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
    public void Validate_ValidAppDto_NoValidationErrors()
    {
        // Arrange
        var appDto = new AppDto
        {
            Name = "ValidAppName",
            Id = Guid.NewGuid()
        };

        // Act
        var result = _validator.Validate(appDto);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_InvalidAppDto_ReturnsValidationErrorsIfNameIsBlank()
    {
        // Arrange
        var appDto = new AppDto
        {
            Name = "", // Invalid: Name is required
            Id = Guid.NewGuid()
        };

        // Act
        var result = _validator.Validate(appDto);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name");
    }

    [Fact]
    public void Validate_InvalidAppDto_ReturnsValidationErrorsIfNameTooLong()
    {
        // Arrange
        var appDto = new AppDto
        {
            Name = new string('A', 256), // Invalid: Name exceeds max length
            Id = Guid.NewGuid()
        };

        // Act
        var result = _validator.Validate(appDto);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name");
    }

    [Fact]
    public void Validate_InvalidAppDto_ReturnsValidationErrorsIfNameHasInvalidCharacters()
    {
        // Arrange
        var appDto = new AppDto
        {
            Name = "Invalid@Name!", // Invalid: Name contains special characters
            Id = Guid.NewGuid()
        };

        // Act
        var result = _validator.Validate(appDto);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name");
    }

    [Fact]
    public void Validate_InvalidAppDto_ReturnsMultipleValidationErrors()
    {
        // Arrange
        var appDto = new AppDto
        {
            Name = new string('@', 256), // Invalid: Too long and has invalid characters
            Id = Guid.NewGuid()
        };

        // Act
        var result = _validator.Validate(appDto);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal(2, result.Errors.Count); // Expecting two errors: Length and Matches
    }
}
