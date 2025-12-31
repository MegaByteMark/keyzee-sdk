using AwesomeAssertions;
using FluentValidation;
using KeyZee.Application.Common.Encryption;
using KeyZee.Application.Common.Persistence;
using KeyZee.Application.Common.Services;
using KeyZee.Application.Services;
using KeyZee.Domain.Models;
using KeyZee.Infrastructure.Encryption;
using KeyZee.Infrastructure.Options;
using NSubstitute;

namespace KeyZee.Tests.Application.Services;

public class KeyValuePairServiceTests
{
    private readonly IKeyZeeUnitOfWork _unitOfWork;
    private readonly IKeyValuePairRepository _mockRepo;
    private readonly IAppService _appService;
    private readonly IKeyValuePairService _systemUnderTest; // System Under Test
    private readonly IValidator<Domain.Models.KeyValuePair> _validator;
    private readonly KeyZeeOptions _options = new(options => { }, "12345678901234567890123456789012", "1234567890123456", "Test");
    private readonly IEncryptionService _encryptionService;

    public KeyValuePairServiceTests()
    {
        _unitOfWork = Substitute.For<IKeyZeeUnitOfWork>();
        _mockRepo = Substitute.For<IKeyValuePairRepository>();
        _unitOfWork.KeyValuePairRepository.Returns(_mockRepo);

        _validator = new KeyZee.Application.Validation.KeyValuePairValidator();
        _appService = Substitute.For<IAppService>();
        _encryptionService = new AesEncryptionService(_options);
        _systemUnderTest = new KeyValuePairService(_unitOfWork, _appService, _options, _validator, _encryptionService);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnApp_WhenAppExists()
    {
        // Arrange
        var kvpId = Guid.NewGuid();
        var appId = Guid.NewGuid();

        var cipherText = _encryptionService.Encrypt("TestValue");

        var kvpEntity = new Domain.Models.KeyValuePair { Id = kvpId, Key = "TestKey", EncryptedValue = cipherText, AppId = appId, Application = new App { Id = appId, Name = "TestApp" } };

        // NSubstitute syntax for setting up a return value
        _mockRepo.GetByIdAsync(Arg.Any<Guid>(), cancellationToken: Arg.Any<CancellationToken>())
                 .Returns(kvpEntity);

        // Act
        var result = await _systemUnderTest.GetByIdAsync(kvpId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Key.Should().Be("TestKey");
        result.Value.EncryptedValue.Should().Be(cipherText);
        result.Value.AppId.Should().Be(appId);

        // NSubstitute syntax for verifying a call
        await _mockRepo.Received(1).GetByIdAsync(Arg.Any<Guid>(), cancellationToken: Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenKeyValuePairDoesNotExist()
    {
        // Arrange
        var kvpId = Guid.NewGuid();
        var appId = Guid.NewGuid();

        _mockRepo.GetByIdAsync(Arg.Any<Guid>(), cancellationToken: Arg.Any<CancellationToken>())
                 .Returns((Domain.Models.KeyValuePair?)null);

        // Act
        var result = await _systemUnderTest.GetByIdAsync(kvpId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Value.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_ShouldCallAddOrUpdateAsync_WhenAppIsValid()
    {
        // Arrange
        var appId = Guid.NewGuid();
        var appName = "NewApp";

        var cipherText = _encryptionService.Encrypt("NewValue");

        var kvp = new Domain.Models.KeyValuePair { Id = Guid.NewGuid(), Key = "NewKey", EncryptedValue = cipherText, AppId = appId };

        _mockRepo.AddOrUpdateAsync(Arg.Any<Domain.Models.KeyValuePair>(), Arg.Any<CancellationToken>())
                 .Returns(Task.CompletedTask);

        _mockRepo.FindAsync(Arg.Any<System.Linq.Expressions.Expression<Func<Domain.Models.KeyValuePair, bool>>>(), cancellationToken: Arg.Any<CancellationToken>())
                 .Returns([]);

        _appService.GetByNameAsync(appName, Arg.Any<CancellationToken>())
                   .Returns(new App { Id = appId, Name = appName });

        // Act
        var result = await _systemUnderTest.CreateAsync(kvp);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        // Verify that AddOrUpdateAsync was called with an App entity that has the correct Name
        await _mockRepo.Received(1).AddOrUpdateAsync(
            Arg.Is<Domain.Models.KeyValuePair>(kvp => kvp.Key == "NewKey" && kvp.AppId == appId),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnError_WhenAppIsInvalid_KeyIsRequired()
    {
        var id = Guid.NewGuid();
        var appId = Guid.NewGuid();

        var cipherText = _encryptionService.Encrypt("SomeValue");

        // Arrange
        var kvp = new Domain.Models.KeyValuePair { Id = id, AppId = appId, Key = "", EncryptedValue = cipherText }; // key is required

        // Act
        var result = await _systemUnderTest.UpdateAsync(kvp);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle().Which.Should().Be("Key is required");

        // Ensure we never touched the DB
        await _mockRepo.DidNotReceive().AddOrUpdateAsync(
            Arg.Any<Domain.Models.KeyValuePair>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnError_WhenAppIsInvalid_KeyIsTooLong()
    {
        // Arrange
        var id = Guid.NewGuid();
        var appId = Guid.NewGuid();

        var cipherText = _encryptionService.Encrypt("SomeValue");

        var kvp = new Domain.Models.KeyValuePair { Id = id, AppId = appId, Key = new string('a', 501), EncryptedValue = cipherText }; // key is too long

        // Act
        var result = await _systemUnderTest.UpdateAsync(kvp);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle().Which.Should().Be("Key cannot exceed 500 characters");

        // Ensure we never touched the DB
        await _mockRepo.DidNotReceive().AddOrUpdateAsync(
            Arg.Any<Domain.Models.KeyValuePair>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnError_WhenAppIsInvalid_ValueIsTooLong()
    {
        // Arrange
        var id = Guid.NewGuid();
        var appId = Guid.NewGuid();
        var cipherText = _encryptionService.Encrypt(new string('a', 5001));
        var kvp = new Domain.Models.KeyValuePair { Id = id, AppId = appId, Key = "SomeKey", EncryptedValue = cipherText }; // value is too long

        // Act
        var result = await _systemUnderTest.UpdateAsync(kvp);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle().Which.Should().Be("Value cannot exceed 5000 characters");

        // Ensure we never touched the DB
        await _mockRepo.DidNotReceive().AddOrUpdateAsync(
            Arg.Any<Domain.Models.KeyValuePair>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteAsync_ShouldCallDelete_WhenKeyValuePairExists()
    {
        // Arrange
        var keyValuePairId = Guid.NewGuid();
        var encryptedValue = _encryptionService.Encrypt("SomeValue");
        var model = new Domain.Models.KeyValuePair { Id = keyValuePairId, Key = "KeyToDelete", EncryptedValue = encryptedValue, AppId = Guid.NewGuid() };
        var keyValuePair = new Domain.Models.KeyValuePair { Id = keyValuePairId, Key = "KeyToDelete", EncryptedValue = encryptedValue, AppId = Guid.NewGuid() };

        _mockRepo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
                 .Returns(model);

        _mockRepo.DeleteAsync(Arg.Any<Domain.Models.KeyValuePair>(), Arg.Any<CancellationToken>())
                 .Returns(Task.CompletedTask);

        // Act
        var result = await _systemUnderTest.DeleteAsync(keyValuePair);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        await _mockRepo.Received(1).DeleteAsync(Arg.Any<Domain.Models.KeyValuePair>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnError_WhenKeyValuePairDoesNotExist()
    {
        // Arrange
        var keyValuePairId = Guid.NewGuid();
        var cipherText = _encryptionService.Encrypt("SomeValue");
        var kvp = new Domain.Models.KeyValuePair { Id = keyValuePairId, Key = "KeyToDelete", EncryptedValue = cipherText, AppId = Guid.NewGuid() };

        _mockRepo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
                 .Returns((Domain.Models.KeyValuePair?)null);

        _mockRepo.DeleteAsync(Arg.Any<Domain.Models.KeyValuePair>(), Arg.Any<CancellationToken>())
                 .Returns(Task.CompletedTask);

        // Act
        var result = await _systemUnderTest.DeleteAsync(kvp);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle().Which.Should().Be("KeyValuePair not found.");

        await _mockRepo.DidNotReceive().DeleteAsync(Arg.Any<Domain.Models.KeyValuePair>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteKeyValuePairByAppAndKeyAsync_ShouldCallDelete_WhenKeyValuePairExists()
    {
        // Arrange
        var keyValuePairId = Guid.NewGuid();
        var encryptedValue = _encryptionService.Encrypt("SomeValue");
        var keyValuePair = new Domain.Models.KeyValuePair { Id = keyValuePairId, Key = "KeyToDelete", EncryptedValue = encryptedValue, AppId = Guid.NewGuid(), Application = new App { Id = Guid.NewGuid(), Name = "SomeApp" } };

        _mockRepo.FindAsync(Arg.Any<System.Linq.Expressions.Expression<Func<Domain.Models.KeyValuePair, bool>>>(), withIncludes: Arg.Any<bool>(), includeDeleted: Arg.Any<bool>(), asNoTracking: Arg.Any<bool>(), cancellationToken: Arg.Any<CancellationToken>())
                 .Returns([keyValuePair]);

        _mockRepo.GetByIdAsync(Arg.Any<Guid>(), withIncludes: Arg.Any<bool>(), asNoTracking: Arg.Any<bool>(), includeDeleted: Arg.Any<bool>(), cancellationToken: Arg.Any<CancellationToken>())
                 .Returns(keyValuePair);

        _mockRepo.DeleteAsync(Arg.Any<Domain.Models.KeyValuePair>(), Arg.Any<CancellationToken>())
                 .Returns(Task.CompletedTask);

        _appService.GetByNameAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                   .Returns(new App { Id = keyValuePair.AppId, Name = "SomeApp" });

        // Act
        var result = await _systemUnderTest.DeleteKeyValuePairByAppAndKeyAsync(keyValuePair.Key);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        await _mockRepo.Received(1).DeleteAsync(Arg.Any<Domain.Models.KeyValuePair>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteKeyValuePairByNameAsync_ShouldReturnError_WhenKeyValuePairDoesNotExist()
    {
        // Arrange
        var keyValuePairId = Guid.NewGuid();
        var encryptedValue = _encryptionService.Encrypt("SomeValue");
        var keyValuePair = new Domain.Models.KeyValuePair { Id = keyValuePairId, Key = "KeyToDelete", EncryptedValue = encryptedValue, AppId = Guid.NewGuid(), Application = new App { Id = Guid.NewGuid(), Name = "SomeApp" } };

        _mockRepo.FindAsync(Arg.Any<System.Linq.Expressions.Expression<Func<Domain.Models.KeyValuePair, bool>>>(), withIncludes: Arg.Any<bool>(), includeDeleted: Arg.Any<bool>(), asNoTracking: Arg.Any<bool>(), cancellationToken: Arg.Any<CancellationToken>())
         .Returns([]);

        _mockRepo.GetByIdAsync(Arg.Any<Guid>(), withIncludes: Arg.Any<bool>(), asNoTracking: Arg.Any<bool>(), includeDeleted: Arg.Any<bool>(), cancellationToken: Arg.Any<CancellationToken>())
                 .Returns(null as Domain.Models.KeyValuePair);

        _mockRepo.DeleteAsync(Arg.Any<Domain.Models.KeyValuePair>(), Arg.Any<CancellationToken>())
                 .Returns(Task.CompletedTask);

        _appService.GetByNameAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                   .Returns(new App { Id = keyValuePair.AppId, Name = "SomeApp" });

        // Act
        var result = await _systemUnderTest.DeleteKeyValuePairByAppAndKeyAsync("NonExistentKey");

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle().Which.Should().Be("KeyValuePair not found.");

        await _mockRepo.DidNotReceive().DeleteAsync(Arg.Any<Domain.Models.KeyValuePair>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateAsync_ShouldMapPropertiesCorrectly()
    {
        // Arrange
        Guid appId = Guid.NewGuid();
        Guid kvpId = Guid.NewGuid();

        string cipherText = _encryptionService.Encrypt("MappedValue");

        var kvp = new Domain.Models.KeyValuePair { AppId = appId, Id = kvpId, Key = "MappedKey", EncryptedValue = cipherText };

        _mockRepo.GetByIdAsync(Arg.Any<Guid>(), cancellationToken: Arg.Any<CancellationToken>()).Returns(null as Domain.Models.KeyValuePair);

        // Act
        var result = await _systemUnderTest.UpdateAsync(kvp);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        await _mockRepo.Received(1).AddOrUpdateAsync(
            Arg.Is<Domain.Models.KeyValuePair>(kvp =>
                kvp.Key == "MappedKey" && kvp.EncryptedValue == cipherText && kvp.AppId == appId),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateAsync_ShouldNotReturnAnError_WhenAppIdAndKeyAlreadyExist()
    {
        // Arrange
        Guid appId = Guid.NewGuid();
        Guid kvpId = Guid.NewGuid();

        string cipherText = _encryptionService.Encrypt("MappedValue");

        var kvp = new Domain.Models.KeyValuePair { AppId = appId, Id = kvpId, Key = "MappedKey", EncryptedValue = cipherText };

        _mockRepo.FindAsync(Arg.Any<System.Linq.Expressions.Expression<Func<Domain.Models.KeyValuePair, bool>>>(), withIncludes: Arg.Any<bool>(), includeDeleted: Arg.Any<bool>(), asNoTracking: Arg.Any<bool>(), cancellationToken: Arg.Any<CancellationToken>())
            .Returns([new Domain.Models.KeyValuePair { Id = Guid.NewGuid(), Key = "MappedKey", EncryptedValue = cipherText, AppId = appId }]);

        // Act
        var result = await _systemUnderTest.UpdateAsync(kvp);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        await _mockRepo.Received(1).AddOrUpdateAsync(
            Arg.Any<Domain.Models.KeyValuePair>(),
            Arg.Any<CancellationToken>());
    }
}
