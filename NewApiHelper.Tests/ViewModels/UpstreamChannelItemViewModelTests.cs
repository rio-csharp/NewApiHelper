using FluentAssertions;
using NewApiHelper.Models;
using NewApiHelper.ViewModels;

namespace NewApiHelper.Tests.ViewModels;

public class UpstreamChannelItemViewModelTests
{
    [Fact]
    public void Constructor_ValidChannel_SetsPropertiesCorrectly()
    {
        // Arrange
        var channel = new Upstream
        {
            Id = 1,
            Name = "Test Channel",
            Url = "https://api.test.com",
            UpstreamRatio = 1.5,
            CreatedAt = new DateTime(2025, 11, 21)
        };

        // Act
        var viewModel = new UpstreamItemViewModel(channel);

        // Assert
        viewModel.Id.Should().Be(1);
        viewModel.Name.Should().Be("Test Channel");
        viewModel.Url.Should().Be("https://api.test.com");
        viewModel.Multiplier.Should().Be(1.5);
        viewModel.CreatedAt.Should().Be(new DateTime(2025, 11, 21));
        viewModel.IsDirty.Should().BeFalse();
        viewModel.IsEditing.Should().BeFalse();
        viewModel.IsNew.Should().BeFalse();
    }

    [Fact]
    public void Name_SetNewValue_UpdatesChannelAndSetsDirty()
    {
        // Arrange
        var channel = new Upstream { Name = "Original Name" };
        var viewModel = new UpstreamItemViewModel(channel);

        // Act
        viewModel.Name = "New Name";

        // Assert
        viewModel.Name.Should().Be("New Name");
        channel.Name.Should().Be("New Name");
        viewModel.IsDirty.Should().BeTrue();
    }

    [Fact]
    public void Name_SetSameValue_DoesNotSetDirty()
    {
        // Arrange
        var channel = new Upstream { Name = "Same Name" };
        var viewModel = new UpstreamItemViewModel(channel);

        // Act
        viewModel.Name = "Same Name";

        // Assert
        viewModel.IsDirty.Should().BeFalse();
    }

    [Fact]
    public void Name_SetNullValue_SetsEmptyString()
    {
        // Arrange
        var channel = new Upstream { Name = "Original" };
        var viewModel = new UpstreamItemViewModel(channel);

        // Act
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        viewModel.Name = null;
#pragma warning restore CS8625

        // Assert
        viewModel.Name.Should().BeEmpty();
        channel.Name.Should().BeEmpty();
        viewModel.IsDirty.Should().BeTrue();
    }

    [Fact]
    public void Url_SetNewValue_UpdatesChannelAndSetsDirty()
    {
        // Arrange
        var channel = new Upstream { Url = "https://original.com" };
        var viewModel = new UpstreamItemViewModel(channel);

        // Act
        viewModel.Url = "https://new.com";

        // Assert
        viewModel.Url.Should().Be("https://new.com");
        channel.Url.Should().Be("https://new.com");
        viewModel.IsDirty.Should().BeTrue();
    }

    [Fact]
    public void Url_SetNullValue_SetsEmptyString()
    {
        // Arrange
        var channel = new Upstream { Url = "https://original.com" };
        var viewModel = new UpstreamItemViewModel(channel);

        // Act
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        viewModel.Url = null;
#pragma warning restore CS8625

        // Assert
        viewModel.Url.Should().BeEmpty();
        channel.Url.Should().BeEmpty();
        viewModel.IsDirty.Should().BeTrue();
    }

    [Fact]
    public void Multiplier_SetNewValue_UpdatesChannelAndSetsDirty()
    {
        // Arrange
        var channel = new Upstream { UpstreamRatio = 1.0 };
        var viewModel = new UpstreamItemViewModel(channel);

        // Act
        viewModel.Multiplier = 2.5;

        // Assert
        viewModel.Multiplier.Should().Be(2.5);
        channel.UpstreamRatio.Should().Be(2.5);
        viewModel.IsDirty.Should().BeTrue();
    }

    [Fact]
    public void Multiplier_SetSameValue_DoesNotSetDirty()
    {
        // Arrange
        var channel = new Upstream { UpstreamRatio = 1.5 };
        var viewModel = new UpstreamItemViewModel(channel);

        // Act
        viewModel.Multiplier = 1.5;

        // Assert
        viewModel.IsDirty.Should().BeFalse();
    }

    [Fact]
    public void IsEditing_SetValue_UpdatesProperty()
    {
        // Arrange
        var channel = new Upstream();
        var viewModel = new UpstreamItemViewModel(channel);

        // Act
        viewModel.IsEditing = true;

        // Assert
        viewModel.IsEditing.Should().BeTrue();
    }

    [Fact]
    public void IsNew_SetValue_UpdatesProperty()
    {
        // Arrange
        var channel = new Upstream();
        var viewModel = new UpstreamItemViewModel(channel);

        // Act
        viewModel.IsNew = true;

        // Assert
        viewModel.IsNew.Should().BeTrue();
    }

    [Fact]
    public void MultiplePropertyChanges_OnlySetsDirtyOnce()
    {
        // Arrange
        var channel = new Upstream
        {
            Name = "Original",
            Url = "https://original.com",
            UpstreamRatio = 1.0
        };
        var viewModel = new UpstreamItemViewModel(channel);

        // Act
        viewModel.Name = "New Name";
        viewModel.Url = "https://new.com";
        viewModel.Multiplier = 2.0;

        // Assert
        viewModel.IsDirty.Should().BeTrue();
        channel.Name.Should().Be("New Name");
        channel.Url.Should().Be("https://new.com");
        channel.UpstreamRatio.Should().Be(2.0);
    }
}