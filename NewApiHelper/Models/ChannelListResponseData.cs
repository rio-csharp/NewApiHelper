using System.Text.Json.Serialization;

namespace NewApiHelper.Models;

public class ChannelListResponseData
{
    [JsonPropertyName("items")]
    public List<Channel> Items { get; set; } = new();

    [JsonPropertyName("total")]
    public int Total { get; set; }
}