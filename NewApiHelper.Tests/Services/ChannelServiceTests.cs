using FluentAssertions;
using Moq;
using Moq.Protected;
using NewApiHelper.Models;
using NewApiHelper.Services;
using System.Collections.ObjectModel;
using System.Net;
using System.Text.Json;

namespace NewApiHelper.Tests.Services;

public class ChannelServiceTests
{
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private readonly HttpClient _httpClient;
    private readonly ChannelService _channelService;

    public ChannelServiceTests()
    {
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();

        // 使用模拟的 handler 创建 HttpClient
        _httpClient = new HttpClient(_mockHttpMessageHandler.Object)
        {
            // 设置一个假的 BaseAddress，因为实际代码中使用了相对路径
            BaseAddress = new Uri("http://fake-newapi.pro")
        };

        _channelService = new ChannelService(_httpClient);
    }

    #region Helper Methods

    // 辅助方法，用于快速设置一个成功的JSON响应
    private void SetupMockHttpResponse(string requestUri, string jsonResponse, HttpMethod method)
    {
        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == method && req.RequestUri!.ToString().Contains(requestUri)),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonResponse)
            });
    }

    // 辅助方法，用于设置一个失败的（非200）响应
    private void SetupMockHttpErrorResponse(string requestUri, HttpMethod method, HttpStatusCode statusCode)
    {
        _mockHttpMessageHandler
           .Protected()
           .Setup<Task<HttpResponseMessage>>(
               "SendAsync",
               ItExpr.Is<HttpRequestMessage>(req =>
                   req.Method == method && req.RequestUri!.ToString().Contains(requestUri)),
               ItExpr.IsAny<CancellationToken>()
           )
           .ReturnsAsync(new HttpResponseMessage
           {
               StatusCode = statusCode
           });
    }

    #endregion Helper Methods

    #region GetChannelsAsync Tests

    [Fact]
    public async Task GetChannelsAsync_ShouldReturnChannelList_WhenApiCallIsSuccessful()
    {
        // Arrange
        var expectedResponse = new ApiResponse<ChannelListResponseData>
        {
            Success = true,
            Data = new ChannelListResponseData
            {
                Items = new List<Channel>
                {
                    new Channel { Id = 1, Name = "Test Channel 1" },
                    new Channel { Id = 2, Name = "Test Channel 2" }
                },
                Total = 2
            }
        };
        var jsonResponse = JsonSerializer.Serialize(expectedResponse);
        SetupMockHttpResponse("/api/channel/?p=1&page_size=20", jsonResponse, HttpMethod.Get);

        // Act
        var result = await _channelService.GetChannelsAsync(1, 20);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Items.Should().HaveCount(2);
        result.Data.Items.First().Name.Should().Be("Test Channel 1");
    }

    [Fact]
    public async Task GetChannelsAsync_ShouldThrowJsonException_WhenResponseIsInvalidJson()
    {
        // Arrange
        SetupMockHttpResponse("/api/channel/?p=1&page_size=20", "invalid json", HttpMethod.Get);

        // Act & Assert
        await Assert.ThrowsAsync<System.Text.Json.JsonException>(async () =>
        {
            await _channelService.GetChannelsAsync(1, 20);
        });
    }

    [Fact]
    public async Task GetChannelsAsync_ShouldThrowHttpRequestException_WhenApiReturnsError()
    {
        // Arrange
        SetupMockHttpErrorResponse("/api/channel/?p=1&page_size=20", HttpMethod.Get, HttpStatusCode.InternalServerError);

        // Act
        Func<Task> act = async () => await _channelService.GetChannelsAsync(1, 20);

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>();
    }

    #endregion GetChannelsAsync Tests

    #region GetChannelByIdAsync Tests

    [Fact]
    public async Task GetChannelByIdAsync_ShouldReturnCorrectChannel_WhenApiCallIsSuccessful()
    {
        // Arrange
        int channelId = 123;
        var expectedResponse = new ApiResponse<Channel>
        {
            Success = true,
            Data = new Channel { Id = channelId, Name = "Specific Channel" }
        };
        var jsonResponse = JsonSerializer.Serialize(expectedResponse);
        SetupMockHttpResponse($"/api/channel/{channelId}", jsonResponse, HttpMethod.Get);

        // Act
        var result = await _channelService.GetChannelByIdAsync(channelId);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Id.Should().Be(channelId);
        result.Data.Name.Should().Be("Specific Channel");
    }

    #endregion GetChannelByIdAsync Tests

    #region TestChannelAsync Tests

    [Fact]
    public async Task TestChannelAsync_ShouldReturnTestResponse_WhenModelIsNotProvided()
    {
        // Arrange
        int channelId = 1;
        var expectedResponse = new TestChannelResponse { Success = true, Time = 1.25 };
        var jsonResponse = JsonSerializer.Serialize(expectedResponse);
        SetupMockHttpResponse($"/api/channel/test/{channelId}", jsonResponse, HttpMethod.Get);

        // Act
        var result = await _channelService.TestChannelAsync(channelId);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Time.Should().Be(1.25);
    }

    [Fact]
    public async Task TestChannelAsync_ShouldCallCorrectUrl_WhenModelIsProvided()
    {
        // Arrange
        int channelId = 1;
        string model = "gpt-4";
        var expectedResponse = new TestChannelResponse { Success = true, Time = 0.9 };
        var jsonResponse = JsonSerializer.Serialize(expectedResponse);
        SetupMockHttpResponse($"/api/channel/test/{channelId}?model={model}", jsonResponse, HttpMethod.Get);

        // Act
        var result = await _channelService.TestChannelAsync(channelId, model);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();

        // 验证是否调用了正确的URL
        _mockHttpMessageHandler.Protected().Verify(
           "SendAsync",
           Times.Once(),
           ItExpr.Is<HttpRequestMessage>(req =>
               req.Method == HttpMethod.Get &&
               req.RequestUri!.ToString().Contains($"/api/channel/test/{channelId}?model={model}")),
           ItExpr.IsAny<CancellationToken>()
       );
    }

    #endregion TestChannelAsync Tests

    #region AddChannelAsync Tests

    [Fact]
    public async Task AddChannelAsync_ShouldReturnSuccess_WhenApiCallIsSuccessful()
    {
        // Arrange
        var newChannel = new AddChannelRequest { Name = "New OpenAI Channel", Type = 1, Key = "sk-123" };
        var expectedResponse = new ApiResponse<object> { Success = true };
        var jsonResponse = JsonSerializer.Serialize(expectedResponse);
        SetupMockHttpResponse("/api/channel/", jsonResponse, HttpMethod.Post);

        HttpRequestMessage? capturedRequest = null;

        _mockHttpMessageHandler
           .Protected()
           .Setup<Task<HttpResponseMessage>>(
               "SendAsync",
               ItExpr.IsAny<HttpRequestMessage>(),
               ItExpr.IsAny<CancellationToken>()
           )
           .Callback<HttpRequestMessage, CancellationToken>((req, token) => capturedRequest = req)
           .ReturnsAsync(new HttpResponseMessage
           {
               StatusCode = HttpStatusCode.OK,
               Content = new StringContent(jsonResponse)
           });

        // Act
        var result = await _channelService.AddChannelAsync(newChannel);

        // Assert
        result.Success.Should().BeTrue();

        // 验证请求体
        capturedRequest.Should().NotBeNull();
        var requestBody = await capturedRequest!.Content!.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(requestBody);
        doc.RootElement.GetProperty("mode").GetString().Should().Be("single");
        doc.RootElement.GetProperty("channel").GetProperty("name").GetString().Should().Be("New OpenAI Channel");
    }

    #endregion AddChannelAsync Tests

    #region UpdateChannelAsync Tests

    [Fact]
    public async Task UpdateChannelAsync_ShouldReturnSuccess_WhenApiCallIsSuccessful()
    {
        // Arrange
        var channelToUpdate = new UpdateChannelRequest { Id = 1, Name = "Updated Channel Name" };
        var expectedResponse = new ApiResponse<object> { Success = true };
        var jsonResponse = JsonSerializer.Serialize(expectedResponse);
        SetupMockHttpResponse("/api/channel/", jsonResponse, HttpMethod.Put);

        // Act
        var result = await _channelService.UpdateChannelAsync(channelToUpdate);

        // Assert
        result.Success.Should().BeTrue();

        // 验证请求方法和URL
        _mockHttpMessageHandler.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req =>
                req.Method == HttpMethod.Put &&
                req.RequestUri!.ToString().Contains("/api/channel/")),
            ItExpr.IsAny<CancellationToken>()
        );
    }

    #endregion UpdateChannelAsync Tests

    #region DeleteChannelAsync Tests

    [Fact]
    public async Task DeleteChannelAsync_ShouldReturnSuccess_WhenApiCallIsSuccessful()
    {
        // Arrange
        int channelId = 456;
        var expectedResponse = new ApiResponse<object> { Success = true };
        var jsonResponse = JsonSerializer.Serialize(expectedResponse);
        SetupMockHttpResponse($"/api/channel/{channelId}", jsonResponse, HttpMethod.Delete);

        // Act
        var result = await _channelService.DeleteChannelAsync(channelId);

        // Assert
        result.Success.Should().BeTrue();

        // 验证请求方法和URL
        _mockHttpMessageHandler.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req =>
                req.Method == HttpMethod.Delete &&
                req.RequestUri!.ToString().Contains($"/api/channel/{channelId}")),
            ItExpr.IsAny<CancellationToken>()
        );
    }

    #endregion DeleteChannelAsync Tests

    #region GenerateChannels Tests

    [Fact]
    public void GenerateChannels_ShouldAssignHigherPriorityValue_ToCheaperChannel()
    {
        // Arrange
        var data = new List<ModelSync>
        {
            CreateSync(1, "gpt-4", price: 10m, usId: 100, grpId: 1), // 便宜: 应获得高分 (100)
            CreateSync(2, "gpt-4", price: 20m, usId: 200, grpId: 2)  // 贵: 应获得低分 (99)
        };
        // Act
        var result = _channelService.GenerateChannels(data).ToList();
        // Assert
        var cheapChannel = result.Single(r => r.BaseUrl.Contains("us100"));
        var expensiveChannel = result.Single(r => r.BaseUrl.Contains("us200"));
        // 验证：便宜的价格 -> Priority 数值更大
        Assert.Equal(100, cheapChannel.Priority);
        Assert.Equal(99, expensiveChannel.Priority);

        // 验证排序：列表第一个应该是高优先级的
        Assert.Equal(cheapChannel, result.First());
    }
    [Fact]
    public void GenerateChannels_ShouldMinimizeGroups_WithDescendingPriority()
    {
        // Arrange
        // 场景：两家价格一致。由于ID排序通过，US1排前面，US2排后面。
        // US1 应该获得 Rank 0 -> Priority 100
        // US2 应该获得 Rank 1 -> Priority 99
        var data = new List<ModelSync>
        {
            CreateSync(1, "model-x", 10m, usId: 1, grpId: 1), // US1
            CreateSync(2, "model-x", 10m, usId: 2, grpId: 1), // US2
            
            CreateSync(3, "model-y", 50m, usId: 1, grpId: 1), // US1
            CreateSync(4, "model-y", 50m, usId: 2, grpId: 1), // US2
        };
        // Act
        var result = _channelService.GenerateChannels(data).ToList();
        // Assert
        Assert.Equal(2, result.Count);
        var p100 = result.Single(r => r.Priority == 100);
        var p99 = result.Single(r => r.Priority == 99);
        Assert.Contains("us1", p100.BaseUrl); // ID小的优先 -> Rank0 -> Priority 100
        Assert.Contains("us2", p99.BaseUrl);  // ID大的靠后 -> Rank1 -> Priority 99

        Assert.Contains("model-x", p100.Models);
        Assert.Contains("model-y", p100.Models);
    }

    // 辅助方法 (保持不变)
    private ModelSync CreateSync(int id, string modelName, decimal price, int usId, int grpId, TestResultStatus status = TestResultStatus.Success)
    {
        var upstream = new Upstream { Id = usId, Name = $"Channel{usId}", Url = $"http://us{usId}.com" };
        var group = new UpstreamGroup
        {
            Id = grpId,
            UpstreamId = usId,
            Name = $"Group{grpId}",
            GroupName = $"Grp{grpId}",
            Key = $"key-{grpId}",
            Upstream = upstream
        };
        return new ModelSync
        {
            Id = id,
            Name = modelName,
            Price = price,
            QuotaType = QuotaType.PayPerUse,
            UpstreamId = usId,
            UpstreamGroupId = grpId,
            Upstream = upstream,
            UpstreamGroup = group,
            TestResults = new ObservableCollection<ModelTestResult> { new ModelTestResult { Status = status } }
        };
    }

    [Fact]
    public void BuildModelMappingForGroupWithFiltering_Should_Map_ValidDateSuffix()
    {
        var modelNames = new[]
        {
            "claude-opus-4-20250514",
            "claude-sonnet-4-20250514",
            "gpt-4.1-2025-04-14"
        };

        var mapping = _channelService.BuildModelMappingForGroupWithFiltering(modelNames);

        Assert.Equal(3, mapping.Count);
        Assert.Equal("claude-opus-4-20250514", mapping["claude-opus-4"]);
        Assert.Equal("claude-sonnet-4-20250514", mapping["claude-sonnet-4"]);
        Assert.Equal("gpt-4.1-2025-04-14", mapping["gpt-4.1"]);
    }

    [Fact]
    public void BuildModelMappingForGroupWithFiltering_Should_NotMap_WhenBaseModelExists()
    {
        var modelNames = new[]
        {
            "gpt-4.1",
            "gpt-4.1-2025-04-14",
            "gpt-5-chat",
            "gpt-5-chat-2025-08-07"
        };

        var mapping = _channelService.BuildModelMappingForGroupWithFiltering(modelNames);

        Assert.Empty(mapping);
    }

    [Fact]
    public void BuildModelMappingForGroupWithFiltering_Should_SelectLatestDate_WhenMultipleVersionsExist()
    {
        var modelNames = new[]
        {
            "gpt-4.1-2025-04-14",
            "gpt-4.1-2025-04-15",
            "gpt-4.1"
        };

        var mapping = _channelService.BuildModelMappingForGroupWithFiltering(modelNames);

        Assert.Empty(mapping);
    }

    [Fact]
    public void BuildModelMappingForGroupWithFiltering_Should_SelectLatestDate_WhenBaseModelNotExist()
    {
        var modelNames = new[]
        {
            "gpt-4.1-2025-04-14",
            "gpt-4.1-2025-04-15"
        };

        var mapping = _channelService.BuildModelMappingForGroupWithFiltering(modelNames);

        Assert.Single(mapping);
        Assert.True(mapping.ContainsKey("gpt-4.1"));
        Assert.Equal("gpt-4.1-2025-04-15", mapping["gpt-4.1"]);
    }

    [Fact]
    public void BuildModelMappingForGroupWithFiltering_Should_HandleMultipleMappings()
    {
        var modelNames = new[]
        {
            "gpt-4.1-2025-04-14",
            "gpt-4.1-2025-04-15",
            "gpt-5-chat-2025-08-07",
            "gpt-5-chat-2025-08-05",
            "claude-opus-4-20251201"
        };

        var mapping = _channelService.BuildModelMappingForGroupWithFiltering(modelNames);

        Assert.Equal(3, mapping.Count);
        Assert.Equal("gpt-4.1-2025-04-15", mapping["gpt-4.1"]);
        Assert.Equal("gpt-5-chat-2025-08-07", mapping["gpt-5-chat"]);
        Assert.Equal("claude-opus-4-20251201", mapping["claude-opus-4"]);
    }

    #endregion
}