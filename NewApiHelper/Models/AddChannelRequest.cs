using System.Text.Json.Serialization;

namespace NewApiHelper.Models;

public class AddChannelRequest
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public int Type { get; set; }

    [JsonPropertyName("key")]
    public string Key { get; set; } = string.Empty;

    [JsonPropertyName("base_url")]
    public string? BaseUrl { get; set; }

    [JsonPropertyName("models")]
    public string? Models { get; set; }

    [JsonPropertyName("groups")]
    public List<string> Groups { get; set; } = new() { "default" };

    [JsonPropertyName("priority")]
    public int? Priority { get; set; }

    [JsonPropertyName("weight")]
    public int? Weight { get; set; }
}