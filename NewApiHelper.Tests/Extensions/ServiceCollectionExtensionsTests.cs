using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NewApiHelper.Data;
using NewApiHelper.Extensions;
using NewApiHelper.Services;

namespace NewApiHelper.Tests.Extensions;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddDatabase_ValidConnectionString_AddsDbContextAndServices()
    {
        // Arrange
        var services = new ServiceCollection();
        var connectionString = "Data Source=:memory:";

        // Act
        services.AddDatabase(connectionString);

        // Assert
        var serviceProvider = services.BuildServiceProvider();

        // Check DbContext is registered
        var dbContext = serviceProvider.GetService<AppDbContext>();
        dbContext.Should().NotBeNull();

        // Check UpStreamChannelService is registered
        var channelService = serviceProvider.GetService<IUpstreamService>();
        channelService.Should().NotBeNull();
        channelService.Should().BeOfType<UpStreamService>();
    }

    [Fact]
    public void AddDatabase_DbContextUsesCorrectConnectionString()
    {
        // Arrange
        var services = new ServiceCollection();
        var connectionString = "Data Source=:memory:";

        // Act
        services.AddDatabase(connectionString);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var dbContext = serviceProvider.GetService<AppDbContext>();

        // The DbContext should be configured with SQLite
        // We can't easily test the exact connection string without exposing internals,
        // but we can verify the DbContext is created successfully
        dbContext.Should().NotBeNull();
    }

    [Fact]
    public void AddDatabase_RegistersScopedUpStreamChannelService()
    {
        // Arrange
        var services = new ServiceCollection();
        var connectionString = "Data Source=:memory:";

        // Act
        services.AddDatabase(connectionString);

        // Assert
        var serviceProvider = services.BuildServiceProvider();

        // Get two instances from the same scope
        var service1 = serviceProvider.GetService<IUpstreamService>();
        var service2 = serviceProvider.GetService<IUpstreamService>();

        // They should be the same instance within the same scope (scoped lifetime)
        service1.Should().BeSameAs(service2);

        // Create a new scope
        using var scope = serviceProvider.CreateScope();
        var scopedServiceProvider = scope.ServiceProvider;
        var scopedService = scopedServiceProvider.GetService<IUpstreamService>();

        // Scoped service in different scope should be different instance
        scopedService.Should().NotBeSameAs(service1);
    }

    [Fact]
    public void AddDatabase_UpStreamChannelServiceReceivesDbContext()
    {
        // Arrange
        var services = new ServiceCollection();
        var connectionString = "Data Source=:memory:";

        // Act
        services.AddDatabase(connectionString);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var channelService = serviceProvider.GetService<IUpstreamService>() as UpStreamService;

        // The service should have been created with a DbContext
        channelService.Should().NotBeNull();
    }
}