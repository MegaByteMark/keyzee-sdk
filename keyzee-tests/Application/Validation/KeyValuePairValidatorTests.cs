using KeyZee.Application.Common.Encryption;
using KeyZee.Application.Validation;
using KeyZee.Infrastructure.Encryption;

namespace KeyZee.Tests.Application.Validation;

public class KeyValuePairValidatorTests
{
    private readonly KeyValuePairValidator _validator;
    private readonly IEncryptionService _encryptionService = new AesEncryptionService(new KeyZee.Infrastructure.Options.KeyZeeOptions()
    {
        EncryptionKey = "12345678901234567890123456789012",
        EncryptionSecret = "1234567890123456",
        AppName = "KeyZeeTests",
        DbContextOptionsBuilder = _ => { }
    });

    public KeyValuePairValidatorTests()
    {
        _validator = new KeyValuePairValidator();
    }

    [Fact]
    public void Validate_ValidKeyValuePairDto_ReturnsNoErrors()
    {
        //Arrange
        var cipherText = _encryptionService.Encrypt("ValidValue");

        var dto = new Domain.Models.KeyValuePair
        {
            AppId = Guid.NewGuid(),
            Key = "ValidKey",
            Id = Guid.NewGuid(),
            EncryptedValue = cipherText,
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
        var cipherText = _encryptionService.Encrypt("ValidValue");

        var dto = new Domain.Models.KeyValuePair
        {
            AppId = Guid.NewGuid(),
            Key = "ValidKey",
            Id = Guid.NewGuid(),
            EncryptedValue = cipherText
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
        var cipherText = _encryptionService.Encrypt("ValidValue");

        var dto = new Domain.Models.KeyValuePair
        {
            AppId = Guid.NewGuid(),
            Key = longKey,
            Id = Guid.NewGuid(),
            EncryptedValue = cipherText
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
        var cipherText = _encryptionService.Encrypt(longValue);

        var dto = new Domain.Models.KeyValuePair
        {
            AppId = Guid.NewGuid(),
            Key = "ValidKey",
            Id = Guid.NewGuid(),
            EncryptedValue = cipherText
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
        var cipherText = _encryptionService.Encrypt("ValidValue");

        var dto = new Domain.Models.KeyValuePair
        {
            AppId = Guid.NewGuid(),
            Key = "",
            Id = Guid.NewGuid(),
            EncryptedValue = cipherText
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
        var cipherText = _encryptionService.Encrypt("ValidValue");

        var dto = new Domain.Models.KeyValuePair
        {
            AppId = Guid.NewGuid(),
            Key = "ValidKey",
            Id = Guid.NewGuid(),
            EncryptedValue = cipherText,
        };

        //Act
        var result = _validator.Validate(dto);

        //Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "AppName" && e.ErrorMessage == "Application name cannot exceed 200 characters");
    }
}
