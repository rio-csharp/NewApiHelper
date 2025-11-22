using System.Text.Json.Serialization;

namespace NewApiHelper.Services;

public class PriceData
{
    [JsonPropertyName("data")]
    public DataContent Data { get; set; } = new();
}

public class DataContent
{
    [JsonPropertyName("group_info")]
    public Dictionary<string, GroupInfo> GroupInfo { get; set; } = new();

    [JsonPropertyName("model_info")]
    public List<ModelInfo> Models { get; set; } = new();
}

public class GroupInfo
{
    [JsonPropertyName("GroupRatio")]
    public double GroupRatio { get; set; }
}

public class ModelInfo
{
    [JsonPropertyName("model_name")]
    public string ModelName { get; set; } = string.Empty;

    [JsonPropertyName("price_info")]
    public Dictionary<string, Dictionary<string, PriceDetail>> PriceInfo { get; set; } = new();

    [JsonPropertyName("enable_groups")]
    public List<string> EnableGroups { get; set; } = new();
}

public class PriceDetail
{
    [JsonPropertyName("model_ratio")]
    public double ModelRatio { get; set; }

    [JsonPropertyName("model_completion_ratio")]
    public double ModelCompletionRatio { get; set; }

    [JsonPropertyName("quota_type")]
    public int QuotaType { get; set; }

    [JsonPropertyName("model_price")]
    public decimal ModelPrice { get; set; }
}