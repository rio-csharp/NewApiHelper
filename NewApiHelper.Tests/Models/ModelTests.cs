using FluentAssertions;
using NewApiHelper.Models;

namespace NewApiHelper.Tests.Models;

public class ChannelTests
{
    [Fact]
    public void Channel_ShouldInitializeWithDefaultValues()
    {
        // Act
        var channel = new Channel();

        // Assert
        channel.Id.Should().Be(0);
        channel.Name.Should().BeEmpty();
        channel.Type.Should().Be(0);
        channel.Status.Should().Be(0);
        channel.Priority.Should().Be(0);
        channel.Weight.Should().Be(0);
        channel.Models.Should().BeEmpty();
        channel.Group.Should().BeEmpty();
        channel.ResponseTime.Should().Be(0);
        channel.TestTime.Should().Be(0);
    }

    [Fact]
    public void Channel_ShouldAllowPropertyAssignment()
    {
        // Arrange
        var channel = new Channel();

        // Act
        channel.Id = 1;
        channel.Name = "Test Channel";
        channel.Type = 1;
        channel.Status = 1;
        channel.Priority = 5;
        channel.Weight = 10;
        channel.BaseUrl = "https://api.test.com";
        channel.Models = "gpt-3.5,gpt-4";
        channel.Key = "test-key";
        channel.Group = "test-group";
        channel.ModelMapping = "test-mapping";
        channel.ResponseTime = 1000;
        channel.TestTime = 1234567890;

        // Assert
        channel.Id.Should().Be(1);
        channel.Name.Should().Be("Test Channel");
        channel.Type.Should().Be(1);
        channel.Status.Should().Be(1);
        channel.Priority.Should().Be(5);
        channel.Weight.Should().Be(10);
        channel.BaseUrl.Should().Be("https://api.test.com");
        channel.Models.Should().Be("gpt-3.5,gpt-4");
        channel.Key.Should().Be("test-key");
        channel.Group.Should().Be("test-group");
        channel.ModelMapping.Should().Be("test-mapping");
        channel.ResponseTime.Should().Be(1000);
        channel.TestTime.Should().Be(1234567890);
    }
}

public class ApiResponseTests
{
    [Fact]
    public void ApiResponse_ShouldInitializeWithDefaultValues()
    {
        // Act
        var response = new ApiResponse<string>();

        // Assert
        response.Success.Should().BeFalse();
        response.Message.Should().BeEmpty();
        response.Data.Should().BeNull();
    }

    [Fact]
    public void ApiResponse_ShouldAllowPropertyAssignment()
    {
        // Arrange
        var response = new ApiResponse<int>();

        // Act
        response.Success = true;
        response.Message = "Success";
        response.Data = 42;

        // Assert
        response.Success.Should().BeTrue();
        response.Message.Should().Be("Success");
        response.Data.Should().Be(42);
    }
}

public class AddChannelRequestTests
{
    [Fact]
    public void AddChannelRequest_ShouldInitializeWithDefaultValues()
    {
        // Act
        var request = new AddChannelRequest();

        // Assert
        request.Name.Should().BeEmpty();
        request.Type.Should().Be(0);
        request.Key.Should().BeEmpty();
        request.BaseUrl.Should().BeNull();
        request.Models.Should().BeNull();
        request.ModelMapping.Should().BeNull();
        request.Groups.Should().Contain("default");
        request.Priority.Should().BeNull();
        request.Weight.Should().BeNull();
    }
}

public class UpdateChannelRequestTests
{
    [Fact]
    public void UpdateChannelRequest_ShouldInitializeWithDefaultValues()
    {
        // Act
        var request = new UpdateChannelRequest();

        // Assert
        request.Id.Should().Be(0);
        request.Name.Should().BeNull();
        request.Status.Should().BeNull();
        request.Priority.Should().BeNull();
        request.Weight.Should().BeNull();
        request.BaseUrl.Should().BeNull();
        request.Models.Should().BeNull();
        request.ModelMapping.Should().BeNull();
        request.Key.Should().BeNull();
        request.Group.Should().BeNull();
    }
}

public class ChannelListResponseDataTests
{
    [Fact]
    public void ChannelListResponseData_ShouldInitializeWithDefaultValues()
    {
        // Act
        var data = new ChannelListResponseData();

        // Assert
        data.Items.Should().NotBeNull();
        data.Items.Should().BeEmpty();
        data.Total.Should().Be(0);
    }
}

public class TestChannelResponseTests
{
    [Fact]
    public void TestChannelResponse_ShouldInitializeWithDefaultValues()
    {
        // Act
        var response = new TestChannelResponse();

        // Assert
        response.Success.Should().BeFalse();
        response.Message.Should().BeEmpty();
        response.Time.Should().Be(0.0);
    }
}