using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NewApiHelper.Data;
using NewApiHelper.Models;
using NewApiHelper.Services;

namespace NewApiHelper.Tests.Services;

public class UpStreamChannelServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly UpStreamChannelService _service;

    public UpStreamChannelServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _service = new UpStreamChannelService(_context);

        // 确保数据库已创建
        _context.Database.EnsureCreated();
    }

    [Fact]
    public async Task GetAllAsync_EmptyDatabase_ReturnsEmptyList()
    {
        // Act
        var result = await _service.GetAllAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task AddAsync_ValidChannel_ReturnsChannelWithId()
    {
        // Arrange
        var channel = new UpStreamChannel
        {
            Name = "Test Channel",
            Url = "https://api.test.com",
            Multiplier = 1.5,
            CreatedAt = DateTime.Now
        };

        // Act
        var result = await _service.AddAsync(channel);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.Name.Should().Be("Test Channel");
        result.Url.Should().Be("https://api.test.com");
        result.Multiplier.Should().Be(1.5);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingChannel_ReturnsChannel()
    {
        // Arrange
        var channel = new UpStreamChannel
        {
            Name = "Test Channel",
            Url = "https://api.test.com",
            Multiplier = 1.0,
            CreatedAt = DateTime.Now
        };
        var addedChannel = await _service.AddAsync(channel);

        // Act
        var result = await _service.GetByIdAsync(addedChannel.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(addedChannel.Id);
        result.Name.Should().Be("Test Channel");
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingChannel_ReturnsNull()
    {
        // Act
        var result = await _service.GetByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_ExistingChannel_UpdatesSuccessfully()
    {
        // Arrange
        var channel = new UpStreamChannel
        {
            Name = "Original Name",
            Url = "https://original.com",
            Multiplier = 1.0,
            CreatedAt = DateTime.Now
        };
        var addedChannel = await _service.AddAsync(channel);

        addedChannel.Name = "Updated Name";
        addedChannel.Url = "https://updated.com";
        addedChannel.Multiplier = 2.0;

        // Act
        var result = await _service.UpdateAsync(addedChannel);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Updated Name");
        result.Url.Should().Be("https://updated.com");
        result.Multiplier.Should().Be(2.0);
    }

    [Fact]
    public async Task DeleteAsync_ExistingChannel_RemovesSuccessfully()
    {
        // Arrange
        var channel = new UpStreamChannel
        {
            Name = "Test Channel",
            Url = "https://api.test.com",
            Multiplier = 1.0,
            CreatedAt = DateTime.Now
        };
        var addedChannel = await _service.AddAsync(channel);

        // Act
        await _service.DeleteAsync(addedChannel.Id);

        // Assert
        var result = await _service.GetByIdAsync(addedChannel.Id);
        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_NonExistingChannel_DoesNotThrow()
    {
        // Act & Assert
        await _service.Invoking(s => s.DeleteAsync(999))
            .Should().NotThrowAsync();
    }

    [Fact]
    public async Task GetAllAsync_MultipleChannels_ReturnsAllChannels()
    {
        // Arrange
        var channel1 = new UpStreamChannel
        {
            Name = "Channel 1",
            Url = "https://api1.com",
            Multiplier = 1.0,
            CreatedAt = DateTime.Now
        };
        var channel2 = new UpStreamChannel
        {
            Name = "Channel 2",
            Url = "https://api2.com",
            Multiplier = 2.0,
            CreatedAt = DateTime.Now
        };

        await _service.AddAsync(channel1);
        await _service.AddAsync(channel2);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(c => c.Name == "Channel 1");
        result.Should().Contain(c => c.Name == "Channel 2");
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}