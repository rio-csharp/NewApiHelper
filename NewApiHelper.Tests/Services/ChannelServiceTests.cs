using FluentAssertions;
using Moq;
using Moq.Protected;
using NewApiHelper.Models;
using NewApiHelper.Services;
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
}