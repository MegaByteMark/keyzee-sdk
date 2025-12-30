using KeyZee.Application.Dtos;
using KeyZee.Application.Validation;

namespace KeyZee.Tests.Application.Validation;

public class KeyValuePairValidatorTests
{
    private readonly KeyValuePairValidator _validator;

    public KeyValuePairValidatorTests()
    {
        _validator = new KeyValuePairValidator();
    }

    [Fact]
    public void Validate_ValidKeyValuePairDto_ReturnsNoErrors()
    {
        //Arrange
        var dto = new KeyValuePairDto
        {
            AppId = Guid.NewGuid(),
            AppName = "ValidApp",
            Key = "ValidKey",
            Value = "ValidValue",
            Id = Guid.NewGuid()
        };

        //Act
        var result = _validator.Validate(dto);

        //Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_EmptyAppName_ReturnsError()
    {
        //Arrange
        var dto = new KeyValuePairDto
        {
            AppId = Guid.NewGuid(),
            AppName = "",
            Key = "ValidKey",
            Value = "ValidValue",
            Id = Guid.NewGuid()
        };

        //Act
        var result = _validator.Validate(dto);

        //Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "AppName" && e.ErrorMessage == "Application name is required");
    }

    [Fact]
    public void Validate_KeyExceedsMaxLength_ReturnsError()
    {
        //Arrange
        var longKey = new string('K', 501);

        var dto = new KeyValuePairDto
        {
            AppId = Guid.NewGuid(),
            AppName = "ValidApp",
            Key = longKey,
            Value = "ValidValue",
            Id = Guid.NewGuid()
        };

        //Act
        var result = _validator.Validate(dto);

        //Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Key" && e.ErrorMessage == "Key cannot exceed 500 characters");
    }

    [Fact]
    public void Validate_ValueExceedsMaxLength_ReturnsError()
    {
        //Arrange
        var longValue = new string('V', 5001);

        var dto = new KeyValuePairDto
        {
            AppId = Guid.NewGuid(),
            AppName = "ValidApp",
            Key = "ValidKey",
            Value = longValue,
            Id = Guid.NewGuid()
        };

        //Act
        var result = _validator.Validate(dto);

        //Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Value" && e.ErrorMessage == "Value cannot exceed 5000 characters");
    }

    [Fact]
    public void Validate_EmptyKey_ReturnsError()
    {
        //Arrange
        var dto = new KeyValuePairDto
        {
            AppId = Guid.NewGuid(),
            AppName = "ValidApp",
            Key = "",
            Value = "ValidValue",
            Id = Guid.NewGuid()
        };

        //Act
        var result = _validator.Validate(dto);

        //Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Key" && e.ErrorMessage == "Key is required");
    }

    [Fact]
    public void Validate_AppNameExceedsMaxLength_ReturnsError()
    {
        //Arrange
        var longAppName = new string('A', 201);

        var dto = new KeyValuePairDto
        {
            AppId = Guid.NewGuid(),
            AppName = longAppName,
            Key = "ValidKey",
            Value = "ValidValue",
            Id = Guid.NewGuid()
        };

        //Act
        var result = _validator.Validate(dto);

        //Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "AppName" && e.ErrorMessage == "Application name cannot exceed 200 characters");
    }
}
