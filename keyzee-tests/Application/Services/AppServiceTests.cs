using AwesomeAssertions;
using FluentValidation;
using KeyZee.Application.Common.Exceptions;
using KeyZee.Application.Common.Persistence;
using KeyZee.Application.Common.Services;
using KeyZee.Application.Dtos;
using KeyZee.Application.Services;
using KeyZee.Domain.Models;
using NSubstitute;

namespace KeyZee.Tests.Application.Services;

public class AppServiceTests
{
    private readonly IKeyZeeUnitOfWork _unitOfWork;
    private readonly IAppRepository _mockRepo;
    private readonly IAppService _systemUnderTest; // System Under Test
    private readonly IValidator<AppDto> _validator;

    public AppServiceTests()
    {
        _unitOfWork = Substitute.For<IKeyZeeUnitOfWork>();
        _mockRepo = Substitute.For<IAppRepository>();
        _unitOfWork.AppRepository.Returns(_mockRepo);

        _validator = new KeyZee.Application.Validation.AppValidator();
        _systemUnderTest = new AppService(_unitOfWork, _validator);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnDto_WhenAppExists()
    {
        // Arrange
        var appId = Guid.NewGuid();
        var appEntity = new App { Id = appId, Name = "Test App" };

        // NSubstitute syntax for setting up a return value
        _mockRepo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<bool>(), cancellationToken: Arg.Any<CancellationToken>())
                 .Returns(appEntity);

        // Act
        var result = await _systemUnderTest.GetByIdAsync(appId);

        // Assert

        result.Should().NotBeNull();
        result.Value.Should().NotBeNull();
        result.Value.Id.Should().Be(appId);
        result.Value.Name.Should().Be("Test App");

        // NSubstitute syntax for verifying a call
        await _mockRepo.Received(1).GetByIdAsync(Arg.Any<Guid>(), cancellationToken: Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenAppDoesNotExist()
    {
        // Arrange
        var appId = Guid.NewGuid();

        _mockRepo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<bool>(), cancellationToken: Arg.Any<CancellationToken>())
                 .Returns((App?)null);

        // Act
        var result = await _systemUnderTest.GetByIdAsync(appId);

        // Assert
        result.Should().NotBeNull();
        result.Value.Should().BeNull();
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task CreateAsync_ShouldCallAddAsync_WhenDtoIsValid()
    {
        // Arrange
        var dto = new AppDto { Name = "NewApp" };

        _mockRepo.AddOrUpdateAsync(Arg.Any<App>(), Arg.Any<CancellationToken>())
                 .Returns(Task.FromResult(new App { Id = Guid.NewGuid(), Name = dto.Name }));

        _mockRepo.FindAsync(Arg.Any<System.Linq.Expressions.Expression<Func<App, bool>>>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<bool>(), cancellationToken: Arg.Any<CancellationToken>())
                 .Returns([]);

        // Act
        await _systemUnderTest.CreateAsync(dto);

        // Assert
        // Verify that AddOrUpdateAsync was called with an App entity that has the correct Name
        await _mockRepo.Received(1).AddOrUpdateAsync(
            Arg.Is<App>(a => a.Name == "NewApp"),
            Arg.Any<CancellationToken>());
    }

        [Fact]
    public async Task CreateAsync_ShouldReturnError_WhenAppNameAlreadyExists()
    {
        // Arrange
        var dto = new AppDto { Id = Guid.NewGuid(), Name = "ExistingApp" };

        // Setup repo to return an existing app when searching by name
        _mockRepo.FindAsync(Arg.Any<System.Linq.Expressions.Expression<Func<App, bool>>>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<bool>(), cancellationToken: Arg.Any<CancellationToken>())
                 .Returns([new() { Id = Guid.NewGuid(), Name = "ExistingApp" }]);

        // Act
        var result = await _systemUnderTest.CreateAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle().Which.Should().Be($"An application with the name '{dto.Name}' already exists.");

        // Ensure we never touched the DB
        await _mockRepo.DidNotReceive().AddOrUpdateAsync(
            Arg.Any<App>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnError_WhenDtoIsInvalid_NameIsRequired()
    {
        // Arrange
        var dto = new AppDto { Name = "" }; // Invalid name

        // Act
        var result = await _systemUnderTest.UpdateAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();

        // Ensure we never touched the DB
        await _mockRepo.DidNotReceive().AddOrUpdateAsync(
            Arg.Any<App>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnError_WhenDtoIsInvalid_NameIsTooLong()
    {
        // Arrange
        var dto = new AppDto { Name = new string('a', 201) }; // long name

        // Act
        var result = await _systemUnderTest.UpdateAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();

        // Ensure we never touched the DB
        await _mockRepo.DidNotReceive().AddOrUpdateAsync(
            Arg.Any<App>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnError_WhenDtoIsInvalid_NameContainsInvalidCharacters()
    {
        // Arrange
        var dto = new AppDto { Name = "Invalid@Name" }; // name with invalid characters

        // Act
        var result = await _systemUnderTest.UpdateAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();

        // Ensure we never touched the DB
        await _mockRepo.DidNotReceive().AddOrUpdateAsync(
            Arg.Any<App>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnError_WhenAppNameAlreadyExists()
    {
        // Arrange
        var dto = new AppDto { Id = Guid.NewGuid(), Name = "ExistingApp" };

        // Setup repo to return an existing app when searching by name
        _mockRepo.FindAsync(Arg.Any<System.Linq.Expressions.Expression<Func<App, bool>>>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<bool>(), cancellationToken: Arg.Any<CancellationToken>())
                 .Returns([new() { Id = Guid.NewGuid(), Name = "ExistingApp" }]);
        // Act
        var result = await _systemUnderTest.UpdateAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle().Which.Should().Be($"An application with the name '{dto.Name}' already exists.");

        // Ensure we never touched the DB
        await _mockRepo.DidNotReceive().AddOrUpdateAsync(
            Arg.Any<App>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteByIdAsync_ShouldCallDelete_WhenAppExists()
    {
        // Arrange
        var appId = Guid.NewGuid();
        var app = new App { Id = appId, Name = "AppToDelete" };

        _mockRepo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<bool>(), cancellationToken: Arg.Any<CancellationToken>())
                 .Returns(app);

        _mockRepo.DeleteAsync(Arg.Any<App>(), Arg.Any<CancellationToken>())
                 .Returns(Task.CompletedTask);

        // Act
        var result = await _systemUnderTest.DeleteByIdAsync(appId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        await _mockRepo.Received(1).DeleteAsync(Arg.Any<App>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteByIdAsync_ShouldThrowNotFound_WhenAppDoesNotExist()
    {
        // Arrange
        var appId = Guid.NewGuid();

        _mockRepo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<bool>(), cancellationToken: Arg.Any<CancellationToken>())
                 .Returns((App?)null);

        // Act
        var result = await _systemUnderTest.DeleteByIdAsync(appId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle().Which.Should().Be("App not found.");

        await _mockRepo.DidNotReceive().DeleteAsync(Arg.Any<App>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteByNameAsync_ShouldCallDelete_WhenAppExists()
    {
        // Arrange
        var appId = Guid.NewGuid();
        var app = new App { Id = appId, Name = "AppToDelete" };

        _mockRepo.FindAsync(Arg.Any<System.Linq.Expressions.Expression<Func<App, bool>>>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<bool>(), cancellationToken: Arg.Any<CancellationToken>())
                 .Returns([app]);

        _mockRepo.DeleteAsync(Arg.Any<App>(), Arg.Any<CancellationToken>())
                 .Returns(Task.CompletedTask);

        _mockRepo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<bool>(), cancellationToken: Arg.Any<CancellationToken>())
                 .Returns(app);

        // Act
        var result = await _systemUnderTest.DeleteByNameAsync(app.Name);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        await _mockRepo.Received(1).DeleteAsync(Arg.Any<App>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteByNameAsync_ShouldThrowNotFound_WhenAppDoesNotExist()
    {
        // Arrange
        var appId = Guid.NewGuid();

        _mockRepo.FindAsync(Arg.Any<System.Linq.Expressions.Expression<Func<App, bool>>>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<bool>(), cancellationToken: Arg.Any<CancellationToken>())
                 .Returns([]);

        // Act
        var result = await _systemUnderTest.DeleteByNameAsync("NonExistentApp");

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle().Which.Should().Be("App not found.");

        await _mockRepo.DidNotReceive().DeleteAsync(Arg.Any<App>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateAsync_ShouldMapPropertiesCorrectly()
    {
        // Arrange
        var dto = new AppDto { Name = "MappedApp" };
        _mockRepo.FindAsync(Arg.Any<System.Linq.Expressions.Expression<Func<App, bool>>>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<bool>(), cancellationToken: Arg.Any<CancellationToken>()).Returns([]);

        // Act
        await _systemUnderTest.UpdateAsync(dto);

        // Assert
        await _mockRepo.Received(1).AddOrUpdateAsync(
            Arg.Is<App>(a => a.Name == "MappedApp" && a.Id != Guid.Empty),
            Arg.Any<CancellationToken>());
    }
}