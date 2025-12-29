using AwesomeAssertions;
using FluentValidation;
using KeyZee.Application.Common.Encryption;
using KeyZee.Application.Common.Persistence;
using KeyZee.Application.Common.Services;
using KeyZee.Application.Dtos;
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
    private readonly IValidator<KeyValuePairDto> _validator;
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
    public async Task GetByIdAsync_ShouldReturnDto_WhenAppExists()
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
        result.Value.Value.Should().Be("TestValue");
        result.Value.AppName.Should().Be("TestApp");

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
    public async Task CreateAsync_ShouldCallAddOrUpdateAsync_WhenDtoIsValid()
    {
        // Arrange
        var appId = Guid.NewGuid();
        var appName = "NewApp";

        var dto = new KeyValuePairDto { Id = Guid.NewGuid(), Key = "NewKey", Value = "NewValue", AppId = appId, AppName = appName };

        _mockRepo.AddOrUpdateAsync(Arg.Any<Domain.Models.KeyValuePair>(), Arg.Any<CancellationToken>())
                 .Returns(Task.CompletedTask);

        _mockRepo.FindAsync(Arg.Any<System.Linq.Expressions.Expression<Func<Domain.Models.KeyValuePair, bool>>>(), cancellationToken: Arg.Any<CancellationToken>())
                 .Returns([]);

        _appService.GetByNameAsync(appName, Arg.Any<CancellationToken>())
                   .Returns(new AppDto { Id = appId, Name = appName });

        // Act
        var result = await _systemUnderTest.CreateAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        // Verify that AddOrUpdateAsync was called with an App entity that has the correct Name
        await _mockRepo.Received(1).AddOrUpdateAsync(
            Arg.Is<Domain.Models.KeyValuePair>(kvp => kvp.Key == "NewKey" && kvp.AppId == appId),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnError_WhenDtoIsInvalid_NameIsTooLong()
    {
        var id = Guid.NewGuid();
        var appId = Guid.NewGuid();

        // Arrange
        var dto = new KeyValuePairDto { Id = id, AppId = appId, Key = "SomeKey", Value = "SomeValue", AppName = new string('a', 201) }; // long name

        // Act
        var result = await _systemUnderTest.UpdateAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle().Which.Should().Be("Application name cannot exceed 200 characters");

        // Ensure we never touched the DB
        await _mockRepo.DidNotReceive().AddOrUpdateAsync(
            Arg.Any<Domain.Models.KeyValuePair>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnError_WhenDtoIsInvalid_KeyIsRequired()
    {
        var id = Guid.NewGuid();
        var appId = Guid.NewGuid();

        // Arrange
        var dto = new KeyValuePairDto { Id = id, AppId = appId, Key = "", Value = "SomeValue", AppName = "SomeApp" }; // key is required

        // Act
        var result = await _systemUnderTest.UpdateAsync(dto);

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
    public async Task UpdateAsync_ShouldReturnError_WhenDtoIsInvalid_KeyIsTooLong()
    {
        // Arrange
        var id = Guid.NewGuid();
        var appId = Guid.NewGuid();

        var dto = new KeyValuePairDto { Id = id, AppId = appId, Key = new string('a', 501), Value = "SomeValue", AppName = "SomeApp" }; // key is too long

        // Act
        var result = await _systemUnderTest.UpdateAsync(dto);

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
    public async Task UpdateAsync_ShouldReturnError_WhenDtoIsInvalid_ValueIsTooLong()
    {
        // Arrange
        var id = Guid.NewGuid();
        var appId = Guid.NewGuid();
        var dto = new KeyValuePairDto { Id = id, AppId = appId, Key = "SomeKey", Value = new string('a', 5001), AppName = "SomeApp" }; // value is too long

        // Act
        var result = await _systemUnderTest.UpdateAsync(dto);

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
        var model = new Domain.Models.KeyValuePair { Id = keyValuePairId, Key = "KeyToDelete", EncryptedValue = "EncryptedValue", AppId = Guid.NewGuid() };
        var keyValuePair = new KeyValuePairDto { Id = keyValuePairId, Key = "KeyToDelete", Value = "EncryptedValue", AppId = Guid.NewGuid(), AppName = "SomeApp" };

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
        var dto = new KeyValuePairDto { Id = keyValuePairId, Key = "KeyToDelete", Value = "EncryptedValue", AppId = Guid.NewGuid(), AppName = "SomeApp" };

        _mockRepo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
                 .Returns((Domain.Models.KeyValuePair?)null);

        _mockRepo.DeleteAsync(Arg.Any<Domain.Models.KeyValuePair>(), Arg.Any<CancellationToken>())
                 .Returns(Task.CompletedTask);

        // Act
        var result = await _systemUnderTest.DeleteAsync(dto);

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
                   .Returns(new AppDto { Id = keyValuePair.AppId, Name = "SomeApp" });

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
                   .Returns(new AppDto { Id = keyValuePair.AppId, Name = "SomeApp" });

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

        var dto = new KeyValuePairDto { AppId = appId, Id = kvpId, Key = "MappedKey", Value = "MappedValue", AppName = "MappedApp" };

        _mockRepo.GetByIdAsync(Arg.Any<Guid>(), cancellationToken: Arg.Any<CancellationToken>()).Returns(null as Domain.Models.KeyValuePair);

        // Act
        await _systemUnderTest.UpdateAsync(dto);

        // Assert
        await _mockRepo.Received(1).AddOrUpdateAsync(
            Arg.Is<Domain.Models.KeyValuePair>(kvp =>
                kvp.Key == "MappedKey" && kvp.EncryptedValue == cipherText && kvp.AppId == appId),
            Arg.Any<CancellationToken>());
    }
}
