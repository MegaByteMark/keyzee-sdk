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
    public async Task GetKeyValuePairAsync_ShouldReturnDto_WhenAppExists()
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
}
