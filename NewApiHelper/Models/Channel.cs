using System.Text.Json.Serialization;

namespace NewApiHelper.Models;

public class Channel
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public int Type { get; set; }

    [JsonPropertyName("status")]
    public int Status { get; set; }

    [JsonPropertyName("priority")]
    public int Priority { get; set; }

    [JsonPropertyName("weight")]
    public int Weight { get; set; }

    [JsonPropertyName("models")]
    public string Models { get; set; } = string.Empty;

    [JsonPropertyName("group")]
    public string Group { get; set; } = string.Empty;

    // --- 仅在列表视图中出现的字段 ---
    [JsonPropertyName("response_time")]
    public int ResponseTime { get; set; }

    [JsonPropertyName("test_time")]
    public long TestTime { get; set; } // Unix timestamp

    // --- 仅在获取单个渠道详情时出现的字段 ---
    [JsonPropertyName("base_url")]
    public string? BaseUrl { get; set; }

    [JsonPropertyName("model_mapping")]
    public string? ModelMapping { get; set; }

    [JsonPropertyName("channel_info")]
    public ChannelInfo? ChannelInfo { get; set; }
}

public class ChannelInfo
{
    [JsonPropertyName("is_multi_key")]
    public bool IsMultiKey { get; set; }

    [JsonPropertyName("multi_key_mode")]
    public string MultiKeyMode { get; set; } = string.Empty;
}