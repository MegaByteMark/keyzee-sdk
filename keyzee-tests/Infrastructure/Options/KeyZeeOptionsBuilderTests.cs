using System;
using System.Reflection;
using KeyZee.Infrastructure.Options;

namespace KeyZee.Tests.Infrastructure;

public class KeyZeeOptionsBuilderTests
{
    private readonly KeyZeeOptionsBuilder _optionsBuilder;

    public KeyZeeOptionsBuilderTests()
    {
        _optionsBuilder = new KeyZeeOptionsBuilder();
    }

    [Fact]
    public void Build_ShouldReturnConfiguredOptions_WhenConfigurationIsProvided()
    {
        // Arrange
        var expectedAppName = "TestApp";
        var expectedEncryptionKey = "TestKey";
        var expectedEncryptionSecret = "TestSecret";
        var action = new Action<Microsoft.EntityFrameworkCore.DbContextOptionsBuilder>(opts => { });

        _optionsBuilder
            .WithAppName(expectedAppName)
            .WithEncryptionKey(expectedEncryptionKey)
            .WithEncryptionSecret(expectedEncryptionSecret)
            .WithDbContextOptions(action);

        // Act
        var options = _optionsBuilder.Build();

        // Assert
        Assert.NotNull(options);
        Assert.Equal(expectedAppName, options.AppName);
        Assert.Equal(expectedEncryptionKey, options.EncryptionKey);
        Assert.Equal(expectedEncryptionSecret, options.EncryptionSecret);
        Assert.Equal(action, options.DbContextOptionsBuilder);
    }

    [Fact]
    public void Build_ShouldReturnDifferentInstances_OnMultipleCalls()
    {
        //Arrange 
        _optionsBuilder
            .WithAppName("TestApp")
            .WithEncryptionKey("TestKey")
            .WithEncryptionSecret("TestSecret")
            .WithDbContextOptions(opts => { });

        // Act
        var options1 = _optionsBuilder.Build();
        var options2 = _optionsBuilder.Build();

        // Assert
        Assert.NotSame(options1, options2);
    }

    [Fact]
    public void WithMethods_ShouldReturnSameBuilderInstance_ForChaining()
    {
        // Act
        var returnedBuilder = _optionsBuilder
            .WithAppName("TestApp")
            .WithEncryptionKey("TestKey")
            .WithEncryptionSecret("TestSecret")
            .WithDbContextOptions(opts => { });

        // Assert
        Assert.Same(_optionsBuilder, returnedBuilder);
    }

    [Fact]
    public void Build_ShouldThrowException_WhenDbContextOptionsNotProvided()
    {
        // Arrange
        _optionsBuilder
            .WithAppName("TestApp")
            .WithEncryptionKey("TestKey")
            .WithEncryptionSecret("TestSecret");

        // Act & Assert
        var exception = Record.Exception(() => _optionsBuilder.Build());

        Assert.NotNull(exception);
        Assert.IsType<InvalidOperationException>(exception);
        Assert.Equal("DbContextOptions must be provided.", exception.Message);
    }

    [Fact]
    public void Build_ShouldThrowException_WhenEncryptionKeyNotProvided()
    {
        // Arrange
        _optionsBuilder
            .WithAppName("TestApp")
            .WithEncryptionSecret("TestSecret")
            .WithDbContextOptions(opts => { });

        // Act & Assert
        var exception = Record.Exception(() => _optionsBuilder.Build());

        Assert.NotNull(exception);
        Assert.IsType<InvalidOperationException>(exception);
        Assert.Equal("Encryption key must be provided.", exception.Message);
    }

    [Fact]
    public void Build_ShouldThrowException_WhenEncryptionSecretNotProvided()
    {
        // Arrange
        _optionsBuilder
            .WithAppName("TestApp")
            .WithEncryptionKey("TestKey")
            .WithDbContextOptions(opts => { });

        // Act & Assert
        var exception = Record.Exception(() => _optionsBuilder.Build());

        Assert.NotNull(exception);
        Assert.IsType<InvalidOperationException>(exception);
        Assert.Equal("Encryption secret must be provided.", exception.Message);
    }

    [Fact]
    public void Build_ShouldUseDefaultAppName_WhenAppNameNotProvided()
    {
        // Arrange
        _optionsBuilder
            .WithEncryptionKey("TestKey")
            .WithEncryptionSecret("TestSecret")
            .WithDbContextOptions(opts => { });

        // Act & Assert
        var exception = Record.Exception(() => _optionsBuilder.Build());

        Assert.Null(exception);

        var options = _optionsBuilder.Build();
        Assert.Equal("keyzee", options.AppName);
    }
}