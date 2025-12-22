using AwesomeAssertions;
using FluentValidation;
using KeyZee.Application.Common.Encryption;
using KeyZee.Application.Common.Exceptions;
using KeyZee.Application.Common.Persistence;
using KeyZee.Application.Common.Services;
using KeyZee.Application.Dtos;
using KeyZee.Application.Services;
using KeyZee.Domain.Models;
using KeyZee.Infrastructure.Encryption;
using KeyZee.Infrastructure.Options;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using NSubstitute;

namespace KeyZee.Tests.Application.Services;

public class KeyValuePairServiceTests
{
    private readonly IKeyValuePairRepository _mockRepo;
    private readonly IAppService _appService;
    private readonly IKeyValuePairService _systemUnderTest; // System Under Test
    private readonly IValidator<KeyValuePairDto> _validator;
    private readonly KeyZeeOptions _options = new(options => { }, "12345678901234567890123456789012", "1234567890123456", "Test");
    private readonly IEncryptionService _encryptionService;

    public KeyValuePairServiceTests()
    {
        _mockRepo = Substitute.For<IKeyValuePairRepository>();
        _validator = new KeyZee.Application.Validation.KeyValuePairValidator();
        _appService = Substitute.For<IAppService>();
        _encryptionService = new AesEncryptionService(_options);
        _systemUnderTest = new KeyValuePairService(_mockRepo, _appService, _options, _validator, _encryptionService);
    }

    [Fact]
    public async Task GetKeyValuePairByIdAsync_ShouldReturnDto_WhenAppExists()
    {
        // Arrange
        var kvpId = Guid.NewGuid();
        var appId = Guid.NewGuid();

        var cipherText = _encryptionService.Encrypt("TestValue");

        var kvpEntity = new Domain.Models.KeyValuePair { Id = kvpId, Key = "TestKey", EncryptedValue = cipherText, AppId = appId, Application = new App { Id = appId, Name = "TestApp" } };

        // NSubstitute syntax for setting up a return value
        _mockRepo.GetAsync(Arg.Any<System.Linq.Expressions.Expression<Func<Domain.Models.KeyValuePair, bool>>>(), cancellationToken: Arg.Any<CancellationToken>())
                 .Returns(kvpEntity);

        // Act
        var result = await _systemUnderTest.GetKeyValuePairByIdAsync(kvpId);

        // Assert
        result.Should().NotBeNull();
        result.Key.Should().Be("TestKey");
        result.Value.Should().Be("TestValue");
        result.AppName.Should().Be("TestApp");

        // NSubstitute syntax for verifying a call
        await _mockRepo.Received(1).GetAsync(Arg.Any<System.Linq.Expressions.Expression<Func<Domain.Models.KeyValuePair, bool>>>(), cancellationToken: Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetKeyValuePairByIdAsync_ShouldReturnNull_WhenKeyValuePairDoesNotExist()
    {
        // Arrange
        var kvpId = Guid.NewGuid();
        var appId = Guid.NewGuid();

        _mockRepo.GetAsync(Arg.Any<System.Linq.Expressions.Expression<Func<Domain.Models.KeyValuePair, bool>>>(), cancellationToken: Arg.Any<CancellationToken>())
                 .Returns((Domain.Models.KeyValuePair?)null);

        // Act
        var result = await _systemUnderTest.GetKeyValuePairByIdAsync(kvpId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateKeyValuePairAsync_ShouldCallAddAsync_WhenDtoIsValid()
    {
        // Arrange
        var appId = Guid.NewGuid();
        var appName = "NewApp";

        var dto = new KeyValuePairDto { Key = "NewKey", Value = "NewValue", AppName = appName };

        _mockRepo.AddOrUpdateAsync(Arg.Any<Domain.Models.KeyValuePair>(), Arg.Any<System.Linq.Expressions.Expression<Func<Domain.Models.KeyValuePair, bool>>>(), Arg.Any<CancellationToken>())
                 .Returns(ValueTask.CompletedTask);

        _mockRepo.FindAsync(Arg.Any<System.Linq.Expressions.Expression<Func<Domain.Models.KeyValuePair, bool>>>(), cancellationToken: Arg.Any<CancellationToken>())
                 .Returns([]);

        _appService.GetAppByNameAsync(appName, Arg.Any<CancellationToken>())
                   .Returns(new AppDto { Id = appId, Name = appName });

        // Act
        await _systemUnderTest.SaveKeyValuePairAsync(dto);

        // Assert
        // Verify that AddOrUpdateAsync was called with an App entity that has the correct Name
        await _mockRepo.Received(1).AddOrUpdateAsync(
            Arg.Is<Domain.Models.KeyValuePair>(kvp => kvp.Key == "NewKey" && kvp.AppId == appId),
            Arg.Any<System.Linq.Expressions.Expression<Func<Domain.Models.KeyValuePair, bool>>>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SaveKeyValuePairAsync_ShouldThrowValidationException_WhenDtoIsInvalid_NameIsTooLong()
    {
        // Arrange
        var dto = new KeyValuePairDto { Key = "SomeKey", Value = "SomeValue", AppName = new string('a', 201) }; // long name
        var validationFailure = new FluentValidation.Results.ValidationFailure("Name", "Application name cannot exceed 200 characters");
        var validationResult = new FluentValidation.Results.ValidationResult([validationFailure]);

        // Act
        Func<Task> act = async () => await _systemUnderTest.SaveKeyValuePairAsync(dto);

        // Assert
        await act.Should().ThrowAsync<ValidationException>();

        // Ensure we never touched the DB
        await _mockRepo.DidNotReceive().AddOrUpdateAsync(
            Arg.Any<Domain.Models.KeyValuePair>(),
            Arg.Any<System.Linq.Expressions.Expression<Func<Domain.Models.KeyValuePair, bool>>>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SaveKeyValuePairAsync_ShouldThrowValidationException_WhenDtoIsInvalid_KeyIsRequired()
    {
        // Arrange
        var dto = new KeyValuePairDto { Key = "", Value = "SomeValue", AppName = "SomeApp" }; // key is required
        var validationFailure = new FluentValidation.Results.ValidationFailure("Key", "Key is required");
        var validationResult = new FluentValidation.Results.ValidationResult([validationFailure]);

        // Act
        Func<Task> act = async () => await _systemUnderTest.SaveKeyValuePairAsync(dto);

        // Assert
        await act.Should().ThrowAsync<ValidationException>();

        // Ensure we never touched the DB
        await _mockRepo.DidNotReceive().AddOrUpdateAsync(
            Arg.Any<Domain.Models.KeyValuePair>(),
            Arg.Any<System.Linq.Expressions.Expression<Func<Domain.Models.KeyValuePair, bool>>>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SaveKeyValuePairAsync_ShouldThrowValidationException_WhenDtoIsInvalid_KeyIsTooLong()
    {
        // Arrange
        var dto = new KeyValuePairDto { Key = new string('a', 501), Value = "SomeValue", AppName = "SomeApp" }; // key is too long
        var validationFailure = new FluentValidation.Results.ValidationFailure("Key", "Key cannot exceed 500 characters");
        var validationResult = new FluentValidation.Results.ValidationResult([validationFailure]);

        // Act
        Func<Task> act = async () => await _systemUnderTest.SaveKeyValuePairAsync(dto);

        // Assert
        await act.Should().ThrowAsync<ValidationException>();

        // Ensure we never touched the DB
        await _mockRepo.DidNotReceive().AddOrUpdateAsync(
            Arg.Any<Domain.Models.KeyValuePair>(),
            Arg.Any<System.Linq.Expressions.Expression<Func<Domain.Models.KeyValuePair, bool>>>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SaveKeyValuePairAsync_ShouldThrowValidationException_WhenDtoIsInvalid_ValueIsTooLong()
    {
        // Arrange
        var dto = new KeyValuePairDto { Key = "SomeKey", Value = new string('a', 5001), AppName = "SomeApp" }; // value is too long
        var validationFailure = new FluentValidation.Results.ValidationFailure("Value", "Value cannot exceed 5000 characters");
        var validationResult = new FluentValidation.Results.ValidationResult([validationFailure]);

        // Act
        Func<Task> act = async () => await _systemUnderTest.SaveKeyValuePairAsync(dto);

        // Assert
        await act.Should().ThrowAsync<ValidationException>();

        // Ensure we never touched the DB
        await _mockRepo.DidNotReceive().AddOrUpdateAsync(
            Arg.Any<Domain.Models.KeyValuePair>(),
            Arg.Any<System.Linq.Expressions.Expression<Func<Domain.Models.KeyValuePair, bool>>>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteKeyValuePairByIdAsync_ShouldCallDelete_WhenKeyValuePairExists()
    {
        // Arrange
        var keyValuePairId = Guid.NewGuid();
        var keyValuePair = new Domain.Models.KeyValuePair { Id = keyValuePairId, Key = "KeyToDelete", EncryptedValue = "EncryptedValue", AppId = Guid.NewGuid() };

        _mockRepo.GetAsync(Arg.Any<System.Linq.Expressions.Expression<Func<Domain.Models.KeyValuePair, bool>>>(), cancellationToken: Arg.Any<CancellationToken>())
                 .Returns(keyValuePair);

        _mockRepo.AddOrUpdateAsync(Arg.Any<Domain.Models.KeyValuePair>(), Arg.Any<System.Linq.Expressions.Expression<Func<Domain.Models.KeyValuePair, bool>>>(), Arg.Any<CancellationToken>())
                 .Returns(ValueTask.CompletedTask);

        // Act
        await _systemUnderTest.DeleteKeyValuePairByIdAsync(keyValuePairId);

        // Assert
        //Calls to AddOrUpdateAsync due to soft deleting
        await _mockRepo.Received(1).AddOrUpdateAsync(Arg.Any<Domain.Models.KeyValuePair>(), Arg.Any<System.Linq.Expressions.Expression<Func<Domain.Models.KeyValuePair, bool>>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteKeyValuePairByIdAsync_ShouldThrowNotFound_WhenKeyValuePairDoesNotExist()
    {
        // Arrange
        var keyValuePairId = Guid.NewGuid();
        var keyValuePair = new Domain.Models.KeyValuePair { Id = keyValuePairId, Key = "KeyToDelete", EncryptedValue = "EncryptedValue", AppId = Guid.NewGuid() };

        _mockRepo.GetAsync(Arg.Any<System.Linq.Expressions.Expression<Func<Domain.Models.KeyValuePair, bool>>>(), cancellationToken: Arg.Any<CancellationToken>())
                 .Returns((Domain.Models.KeyValuePair?)null);

        _mockRepo.AddOrUpdateAsync(Arg.Any<Domain.Models.KeyValuePair>(), Arg.Any<System.Linq.Expressions.Expression<Func<Domain.Models.KeyValuePair, bool>>>(), Arg.Any<CancellationToken>())
                 .Returns(ValueTask.CompletedTask);

        // Act
        Func<Task> act = async () => await _systemUnderTest.DeleteKeyValuePairByIdAsync(keyValuePairId);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>(); // Or your custom NotFoundException

        //Calls to AddOrUpdateAsync due to soft deleting
        await _mockRepo.DidNotReceive().AddOrUpdateAsync(Arg.Any<Domain.Models.KeyValuePair>(), Arg.Any<System.Linq.Expressions.Expression<Func<Domain.Models.KeyValuePair, bool>>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteKeyValuePairByNameAsync_ShouldCallDelete_WhenKeyValuePairExists()
    {
        // Arrange
        var keyValuePairId = Guid.NewGuid();
        var keyValuePair = new Domain.Models.KeyValuePair { Id = keyValuePairId, Key = "KeyToDelete", EncryptedValue = "EncryptedValue", AppId = Guid.NewGuid() };

        _mockRepo.FindAsync(Arg.Any<System.Linq.Expressions.Expression<Func<Domain.Models.KeyValuePair, bool>>>(), cancellationToken: Arg.Any<CancellationToken>())
                 .Returns(new List<Domain.Models.KeyValuePair> { keyValuePair });

        _mockRepo.AddOrUpdateAsync(Arg.Any<Domain.Models.KeyValuePair>(), Arg.Any<System.Linq.Expressions.Expression<Func<Domain.Models.KeyValuePair, bool>>>(), Arg.Any<CancellationToken>())
                 .Returns(ValueTask.CompletedTask);

        // Act
        await _systemUnderTest.DeleteKeyValuePairByAppAndKeyAsync(keyValuePair.Key);

        // Assert
        //Calls to AddOrUpdateAsync due to soft deleting
        await _mockRepo.Received(1).AddOrUpdateAsync(Arg.Any<Domain.Models.KeyValuePair>(), Arg.Any<System.Linq.Expressions.Expression<Func<Domain.Models.KeyValuePair, bool>>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteKeyValuePairByNameAsync_ShouldThrowNotFound_WhenKeyValuePairDoesNotExist()
    {
        // Arrange
        var keyValuePairId = Guid.NewGuid();
        var keyValuePair = new Domain.Models.KeyValuePair { Id = keyValuePairId, Key = "KeyToDelete", EncryptedValue = "EncryptedValue", AppId = Guid.NewGuid() };

        _mockRepo.GetAsync(Arg.Any<System.Linq.Expressions.Expression<Func<Domain.Models.KeyValuePair, bool>>>(), cancellationToken: Arg.Any<CancellationToken>())
                 .Returns((Domain.Models.KeyValuePair?)null);

        _mockRepo.AddOrUpdateAsync(Arg.Any<Domain.Models.KeyValuePair>(), Arg.Any<System.Linq.Expressions.Expression<Func<Domain.Models.KeyValuePair, bool>>>(), Arg.Any<CancellationToken>())
                 .Returns(ValueTask.CompletedTask);

        // Act
        Func<Task> act = async () => await _systemUnderTest.DeleteKeyValuePairByAppAndKeyAsync("NonExistentKey");

        // Assert
        await act.Should().ThrowAsync<NotFoundException>(); // Or your custom NotFoundException

        //Calls to AddOrUpdateAsync due to soft deleting
        await _mockRepo.DidNotReceive().AddOrUpdateAsync(Arg.Any<Domain.Models.KeyValuePair>(), Arg.Any<System.Linq.Expressions.Expression<Func<Domain.Models.KeyValuePair, bool>>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SaveKeyValuePairAsync_ShouldMapPropertiesCorrectly()
    {
        // Arrange
        IEncryptionService encryptionService = new AesEncryptionService(_options);
        string cipherText = encryptionService.Encrypt("MappedValue");

        var dto = new KeyValuePairDto { Key = "MappedKey", Value = "MappedValue", AppName = "MappedApp"};
        _mockRepo.FindAsync(Arg.Any<System.Linq.Expressions.Expression<Func<Domain.Models.KeyValuePair, bool>>>(), cancellationToken: Arg.Any<CancellationToken>()).Returns([]);

        // Act
        await _systemUnderTest.SaveKeyValuePairAsync(dto);

        // Assert
        await _mockRepo.Received(1).AddOrUpdateAsync(
            Arg.Is<Domain.Models.KeyValuePair>(kvp =>
                kvp.Key == "MappedKey"),
            Arg.Any<System.Linq.Expressions.Expression<Func<Domain.Models.KeyValuePair, bool>>>(),
            Arg.Any<CancellationToken>());
    }
}
