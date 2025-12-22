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
    private readonly IAppRepository _mockRepo;
    private readonly IAppService _systemUnderTest; // System Under Test
    private readonly IValidator<AppDto> _validator;

    public AppServiceTests()
    {
        _mockRepo = Substitute.For<IAppRepository>();
        _validator = new KeyZee.Application.Validation.AppValidator();
        _systemUnderTest = new AppService(_mockRepo, _validator);
    }

    [Fact]
    public async Task GetAppAsync_ShouldReturnDto_WhenAppExists()
    {
        // Arrange
        var appId = Guid.NewGuid();
        var appEntity = new App { Id = appId, Name = "Test App" };

        // NSubstitute syntax for setting up a return value
        _mockRepo.GetAsync(Arg.Any<System.Linq.Expressions.Expression<System.Func<App, bool>>>(), cancellationToken: Arg.Any<CancellationToken>())
                 .Returns(appEntity);

        // Act
        var result = await _systemUnderTest.GetAppByIdAsync(appId);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Test App");

        // NSubstitute syntax for verifying a call
        await _mockRepo.Received(1).GetAsync(Arg.Any<System.Linq.Expressions.Expression<System.Func<App, bool>>>(), cancellationToken: Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetAppAsync_ShouldReturnNull_WhenAppDoesNotExist()
    {
        // Arrange
        var appId = Guid.NewGuid();

        _mockRepo.GetAsync(Arg.Any<System.Linq.Expressions.Expression<Func<App, bool>>>(), cancellationToken: Arg.Any<CancellationToken>())
                 .Returns((App?)null);

        // Act
        var result = await _systemUnderTest.GetAppByIdAsync(appId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAppAsync_ShouldCallAddAsync_WhenDtoIsValid()
    {
        // Arrange
        var dto = new AppDto { Name = "NewApp" };

        _mockRepo.AddOrUpdateAsync(Arg.Any<App>(), Arg.Any<System.Linq.Expressions.Expression<Func<App, bool>>>(), Arg.Any<CancellationToken>())
                 .Returns(ValueTask.CompletedTask);

        _mockRepo.FindAsync(Arg.Any<System.Linq.Expressions.Expression<Func<App, bool>>>(), cancellationToken: Arg.Any<CancellationToken>())
                 .Returns(new List<App>());

        // Act
        await _systemUnderTest.SaveAppAsync(dto);

        // Assert
        // Verify that AddOrUpdateAsync was called with an App entity that has the correct Name
        await _mockRepo.Received(1).AddOrUpdateAsync(
            Arg.Is<App>(a => a.Name == "NewApp"),
            Arg.Any<System.Linq.Expressions.Expression<Func<App, bool>>>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SaveAppAsync_ShouldThrowValidationException_WhenDtoIsInvalid_NameIsRequired()
    {
        // Arrange
        var dto = new AppDto { Name = "" }; // Invalid name
        var validationFailure = new FluentValidation.Results.ValidationFailure("Name", "Application name is required");
        var validationResult = new FluentValidation.Results.ValidationResult([validationFailure]);

        // Act
        Func<Task> act = async () => await _systemUnderTest.SaveAppAsync(dto);

        // Assert
        await act.Should().ThrowAsync<ValidationException>();

        // Ensure we never touched the DB
        await _mockRepo.DidNotReceive().AddOrUpdateAsync(
            Arg.Any<App>(),
            Arg.Any<System.Linq.Expressions.Expression<Func<App, bool>>>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SaveAppAsync_ShouldThrowValidationException_WhenDtoIsInvalid_NameIsTooLong()
    {
        // Arrange
        var dto = new AppDto { Name = new string('a', 201) }; // long name
        var validationFailure = new FluentValidation.Results.ValidationFailure("Name", "Application name cannot exceed 200 characters");
        var validationResult = new FluentValidation.Results.ValidationResult([validationFailure]);

        // Act
        Func<Task> act = async () => await _systemUnderTest.SaveAppAsync(dto);

        // Assert
        await act.Should().ThrowAsync<ValidationException>();

        // Ensure we never touched the DB
        await _mockRepo.DidNotReceive().AddOrUpdateAsync(
            Arg.Any<App>(),
            Arg.Any<System.Linq.Expressions.Expression<Func<App, bool>>>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SaveAppAsync_ShouldThrowValidationException_WhenDtoIsInvalid_NameContainsInvalidCharacters()
    {
        // Arrange
        var dto = new AppDto { Name = "Invalid@Name" }; // name with invalid characters
        var validationFailure = new FluentValidation.Results.ValidationFailure("Name", "Application name can only contain letters, numbers, underscores, and hyphens");
        var validationResult = new FluentValidation.Results.ValidationResult([validationFailure]);

        // Act
        Func<Task> act = async () => await _systemUnderTest.SaveAppAsync(dto);

        // Assert
        await act.Should().ThrowAsync<ValidationException>();

        // Ensure we never touched the DB
        await _mockRepo.DidNotReceive().AddOrUpdateAsync(
            Arg.Any<App>(),
            Arg.Any<System.Linq.Expressions.Expression<Func<App, bool>>>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SaveAppAsync_ShouldThrowException_WhenAppNameAlreadyExists()
    {
        // Arrange
        var dto = new AppDto { Name = "ExistingApp" };

        // Setup repo to return an existing app when searching by name
        _mockRepo.FindAsync(Arg.Any<System.Linq.Expressions.Expression<Func<App, bool>>>(), cancellationToken: Arg.Any<CancellationToken>())
                 .Returns(new List<App> { new App { Name = "ExistingApp" } });
        // Act
        Func<Task> act = async () => await _systemUnderTest.SaveAppAsync(dto);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>() // Or your custom DuplicateException
                 .WithMessage($"An application with the name '{dto.Name}' already exists.");

        await _mockRepo.DidNotReceive().AddOrUpdateAsync(
            Arg.Any<App>(),
            Arg.Any<System.Linq.Expressions.Expression<Func<App, bool>>>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteAppByIdAsync_ShouldCallDelete_WhenAppExists()
    {
        // Arrange
        var appId = Guid.NewGuid();
        var app = new App { Id = appId, Name = "AppToDelete" };

        _mockRepo.GetAsync(Arg.Any<System.Linq.Expressions.Expression<Func<App, bool>>>(), cancellationToken: Arg.Any<CancellationToken>())
                 .Returns(app);

        _mockRepo.AddOrUpdateAsync(Arg.Any<App>(), Arg.Any<System.Linq.Expressions.Expression<Func<App, bool>>>(), Arg.Any<CancellationToken>())
                 .Returns(ValueTask.CompletedTask);

        // Act
        await _systemUnderTest.DeleteAppByIdAsync(appId);

        // Assert
        //Calls to AddOrUpdateAsync due to soft deleting
        await _mockRepo.Received(1).AddOrUpdateAsync(Arg.Any<App>(), Arg.Any<System.Linq.Expressions.Expression<Func<App, bool>>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteAppByIdAsync_ShouldThrowNotFound_WhenAppDoesNotExist()
    {
        // Arrange
        var appId = Guid.NewGuid();

        _mockRepo.GetAsync(Arg.Any<System.Linq.Expressions.Expression<Func<App, bool>>>(), cancellationToken: Arg.Any<CancellationToken>())
                 .Returns((App?)null);

        // Act
        Func<Task> act = async () => await _systemUnderTest.DeleteAppByIdAsync(appId);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>(); // Or your custom NotFoundException

        //Calls to AddOrUpdateAsync due to soft deleting
        await _mockRepo.DidNotReceive().AddOrUpdateAsync(Arg.Any<App>(), Arg.Any<System.Linq.Expressions.Expression<Func<App, bool>>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteAppByNameAsync_ShouldCallDelete_WhenAppExists()
    {
        // Arrange
        var appId = Guid.NewGuid();
        var app = new App { Id = appId, Name = "AppToDelete" };

        _mockRepo.FindAsync(Arg.Any<System.Linq.Expressions.Expression<Func<App, bool>>>(), cancellationToken: Arg.Any<CancellationToken>())
                 .Returns([app]);

        _mockRepo.AddOrUpdateAsync(Arg.Any<App>(), Arg.Any<System.Linq.Expressions.Expression<Func<App, bool>>>(), Arg.Any<CancellationToken>())
                 .Returns(ValueTask.CompletedTask);

        // Act
        await _systemUnderTest.DeleteAppByNameAsync(app.Name);

        // Assert
        //Calls to AddOrUpdateAsync due to soft deleting
        await _mockRepo.Received(1).AddOrUpdateAsync(Arg.Any<App>(), Arg.Any<System.Linq.Expressions.Expression<Func<App, bool>>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteAppByNameAsync_ShouldThrowNotFound_WhenAppDoesNotExist()
    {
        // Arrange
        var appId = Guid.NewGuid();

        _mockRepo.FindAsync(Arg.Any<System.Linq.Expressions.Expression<Func<App, bool>>>(), cancellationToken: Arg.Any<CancellationToken>())
                 .Returns([]);

        // Act
        Func<Task> act = async () => await _systemUnderTest.DeleteAppByNameAsync("NonExistentApp");

        // Assert
        await act.Should().ThrowAsync<NotFoundException>(); // Or your custom NotFoundException

        //Calls to AddOrUpdateAsync due to soft deleting
        await _mockRepo.DidNotReceive().AddOrUpdateAsync(Arg.Any<App>(), Arg.Any<System.Linq.Expressions.Expression<Func<App, bool>>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SaveAppAsync_ShouldMapPropertiesCorrectly()
    {
        // Arrange
        var dto = new AppDto { Name = "MappedApp"};
        _mockRepo.FindAsync(Arg.Any<System.Linq.Expressions.Expression<Func<App, bool>>>(), cancellationToken: Arg.Any<CancellationToken>()).Returns([]);

        // Act
        await _systemUnderTest.SaveAppAsync(dto);

        // Assert
        await _mockRepo.Received(1).AddOrUpdateAsync(
            Arg.Is<App>(a =>
                a.Name == "MappedApp" && a.Id != Guid.Empty),
            Arg.Any<System.Linq.Expressions.Expression<Func<App, bool>>>(),
            Arg.Any<CancellationToken>());
    }
}