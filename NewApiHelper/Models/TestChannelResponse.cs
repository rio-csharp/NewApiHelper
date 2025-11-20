using System.Text.Json.Serialization;

namespace NewApiHelper.Models;

public class TestChannelResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("time")]
    public double Time { get; set; } // 响应时间（秒）
}