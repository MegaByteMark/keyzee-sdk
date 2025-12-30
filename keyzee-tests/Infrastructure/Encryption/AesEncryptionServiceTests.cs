using System;
using AwesomeAssertions;
using KeyZee.Application.Common.Encryption;
using KeyZee.Infrastructure.Encryption;
using KeyZee.Infrastructure.Options;

namespace KeyZee.Tests.Infrastructure.Encryption;

public class AesEncryptionServiceTests
{
    private readonly IEncryptionService _encryptionService;

    public AesEncryptionServiceTests()
    {
        var optionsBuilder = new KeyZeeOptionsBuilder()
            .WithEncryptionKey("12345678901234567890123456789012")
            .WithEncryptionSecret("1234567890123456")
            .WithDbContextOptions(opts => { });

        var options = optionsBuilder.Build();

        _encryptionService = new AesEncryptionService(options);
    }

    [Fact]
    public void Encrypt_Then_Decrypt_Returns_Original_Text()
    {
        // Arrange
        var originalText = "Hello, World!";

        // Act
        var encryptedText = _encryptionService.Encrypt(originalText);
        var decryptedText = _encryptionService.Decrypt(encryptedText);

        // Assert
        decryptedText.Should().Be(originalText);
    }

    [Fact]
    public void Decrypt_Invalid_CipherText_Throws_Exception()
    {
        // Arrange
        var invalidCipherText = "InvalidCipherText";

        // Act & Assert
        Action act = () => _encryptionService.Decrypt(invalidCipherText);
        act.Should().Throw<FormatException>();
    }

    [Fact]
    public void Encrypt_Null_Text_Throws_Exception()
    {
        // Arrange
        string nullText = null!;

        // Act & Assert
        Action act = () => _encryptionService.Encrypt(nullText);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Decrypt_Null_CipherText_Throws_Exception()
    {
        // Arrange
        string nullCipherText = null!;

        // Act & Assert
        Action act = () => _encryptionService.Decrypt(nullCipherText);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Encrypt_Empty_String_Returns_Empty_String()
    {
        // Arrange
        var emptyString = string.Empty;

        // Act
        var encryptedText = _encryptionService.Encrypt(emptyString);
        var decryptedText = _encryptionService.Decrypt(encryptedText);

        // Assert
        decryptedText.Should().Be(emptyString);
    }

    [Fact]
    public void Encrypt_Long_Text_Works_Correctly()
    {
        // Arrange
        var longText = new string('A', 10000); // 10,000 characters of 'A'

        // Act
        var encryptedText = _encryptionService.Encrypt(longText);
        var decryptedText = _encryptionService.Decrypt(encryptedText);

        // Assert
        decryptedText.Should().Be(longText);
    }
}
