using Microsoft.EntityFrameworkCore;
using NewApiHelper.Data;
using NewApiHelper.Models;
using NewApiHelper.Services;

namespace NewApiHelper.Tests.Services;

public class UpstreamGroupServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly UpstreamGroupService _service;

    public UpstreamGroupServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _service = new UpstreamGroupService(_context);

        // Seed test data
        SeedDatabase();
    }

    private void SeedDatabase()
    {
        var upstream = new Upstream
        {
            Id = 1,
            Name = "Test Upstream",
            Url = "http://test.com",
            UpstreamRatio = 1.0
        };

        _context.UpStreams.Add(upstream);

        var group1 = new UpstreamGroup
        {
            Id = 1,
            GroupName = "Group 1",
            GroupRatio = 1.0,
            UpstreamId = 1,
            Upstream = upstream
        };

        var group2 = new UpstreamGroup
        {
            Id = 2,
            GroupName = "Group 2",
            GroupRatio = 0.5,
            UpstreamId = 1,
            Upstream = upstream
        };

        _context.UpstreamGroups.AddRange(group1, group2);
        _context.SaveChanges();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllGroups()
    {
        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, g => Assert.NotNull(g.Upstream));
    }

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsGroup()
    {
        // Act
        var result = await _service.GetByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result!.Id);
        Assert.Equal("Group 1", result.GroupName);
        Assert.NotNull(result.Upstream);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingId_ReturnsNull()
    {
        // Act
        var result = await _service.GetByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task AddAsync_AddsGroup()
    {
        // Arrange
        var newGroup = new UpstreamGroup
        {
            GroupName = "New Group",
            GroupRatio = 2.0,
            UpstreamId = 1
        };

        // Act
        var result = await _service.AddAsync(newGroup);

        // Assert
        Assert.Equal("New Group", result.GroupName);
        Assert.Equal(2.0, result.GroupRatio);

        var savedGroup = await _context.UpstreamGroups.FindAsync(result.Id);
        Assert.NotNull(savedGroup);
        Assert.Equal("New Group", savedGroup.GroupName);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesGroup()
    {
        // Arrange
        var group = await _context.UpstreamGroups.FindAsync(1);
        Assert.NotNull(group);
        group.GroupName = "Updated Group";
        group.GroupRatio = 1.5;

        // Act
        var result = await _service.UpdateAsync(group);

        // Assert
        Assert.Equal("Updated Group", result.GroupName);
        Assert.Equal(1.5, result.GroupRatio);

        var updatedGroup = await _context.UpstreamGroups.FindAsync(1);
        Assert.NotNull(updatedGroup);
        Assert.Equal("Updated Group", updatedGroup.GroupName);
    }

    [Fact]
    public async Task DeleteAsync_ExistingId_DeletesGroup()
    {
        // Act
        await _service.DeleteAsync(1);

        // Assert
        var deletedGroup = await _context.UpstreamGroups.FindAsync(1);
        Assert.Null(deletedGroup);
    }

    [Fact]
    public async Task DeleteAsync_NonExistingId_DoesNothing()
    {
        // Act
        await _service.DeleteAsync(999);

        // Assert - No exception should be thrown
        var groupsCount = await _context.UpstreamGroups.CountAsync();
        Assert.Equal(2, groupsCount);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}