using NewApiHelper.Models;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Web;

namespace NewApiHelper.Services;

// https://docs.newapi.pro/api/fei-channel-management/#_25
public class ChannelService : IChannelService
{
    private readonly HttpClient _httpClient;

    public ChannelService(HttpClient client)
    {
        _httpClient = client;
    }

    public async Task<ApiResponse<ChannelListResponseData>> GetChannelsAsync(int page = 1, int pageSize = 20)
    {
        var query = HttpUtility.ParseQueryString(string.Empty);
        query["p"] = page.ToString();
        query["page_size"] = pageSize.ToString();

        var response = await _httpClient.GetAsync($"/api/channel/?{query}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ApiResponse<ChannelListResponseData>>()
               ?? new ApiResponse<ChannelListResponseData> { Success = false, Message = "Failed to deserialize response." };
    }

    public async Task<ApiResponse<Channel>> GetChannelByIdAsync(int id)
    {
        var response = await _httpClient.GetAsync($"/api/channel/{id}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ApiResponse<Channel>>()
             ?? new ApiResponse<Channel> { Success = false, Message = "Failed to deserialize response." };
    }

    public async Task<TestChannelResponse> TestChannelAsync(int id, string? model = null)
    {
        string requestUri = $"/api/channel/test/{id}";
        if (!string.IsNullOrEmpty(model))
        {
            requestUri += $"?model={model}";
        }

        var response = await _httpClient.GetAsync(requestUri);

        return await response.Content.ReadFromJsonAsync<TestChannelResponse>()
               ?? new TestChannelResponse { Success = false, Message = "Failed to deserialize test response." };
    }

    public async Task<ApiResponse<object>> AddChannelAsync(AddChannelRequest newChannel)
    {
        var requestBody = new
        {
            mode = "single",
            channel = newChannel
        };

        var response = await _httpClient.PostAsJsonAsync("/api/channel/", requestBody);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<ApiResponse<object>>()
            ?? new ApiResponse<object> { Success = false, Message = "Failed to deserialize response." };
    }

    public async Task<ApiResponse<object>> UpdateChannelAsync(UpdateChannelRequest channelToUpdate)
    {
        var response = await _httpClient.PutAsJsonAsync("/api/channel/", channelToUpdate);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ApiResponse<object>>()
             ?? new ApiResponse<object> { Success = false, Message = "Failed to deserialize response." };
    }

    public async Task<ApiResponse<object>> DeleteChannelAsync(int id)
    {
        var response = await _httpClient.DeleteAsync($"/api/channel/{id}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ApiResponse<object>>()
             ?? new ApiResponse<object> { Success = false, Message = "Failed to deserialize response." };
    }

    public async Task<ApiResponse<int>> DeleteChannelsAsync(IEnumerable<int> ids)
    {
        var requestBody = new
        {
            ids = ids.ToArray()
        };

        var response = await _httpClient.PostAsJsonAsync("/api/channel/batch", requestBody);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<ApiResponse<int>>()
            ?? new ApiResponse<int> { Success = false, Message = "Failed to deserialize response." };
    }

    private const int MaxPriority = 100;
    private readonly struct ChannelKey : IEquatable<ChannelKey>
    {
        public int UpstreamId { get; }
        public int GroupId { get; }
        public int Priority { get; }
        public ChannelKey(int uId, int gId, int priority)
        {
            UpstreamId = uId;
            GroupId = gId;
            Priority = priority;
        }
        public bool Equals(ChannelKey other) =>
            UpstreamId == other.UpstreamId && GroupId == other.GroupId && Priority == other.Priority;
        public override int GetHashCode() => HashCode.Combine(UpstreamId, GroupId, Priority);
    }
    public IEnumerable<AddChannelRequest> GenerateChannels(IEnumerable<ModelSync> modelSyncs)
    {
        if (modelSyncs == null) return Enumerable.Empty<AddChannelRequest>();
        var validSyncs = modelSyncs
            .Where(m => m.LatestTestResult == TestResultStatus.Success
                        && m.Upstream != null
                        && m.UpstreamGroup != null)
            .ToList();
        if (!validSyncs.Any()) return Enumerable.Empty<AddChannelRequest>();
        var groupedByModel = validSyncs.GroupBy(m => m.Name);
        var channelBuckets = new Dictionary<ChannelKey, List<ModelSync>>();
        var channelMetaCache = new Dictionary<(int UId, int GId), (string BaseUrl, string Key, string UName, string GName)>();
        foreach (var modelGroup in groupedByModel)
        {
            // 排序逻辑保持不变：
            // 1. 价格更低 -> 排在前面 (Index 小)
            // 2. UpstreamId 更小 -> 排在前面 (Tie-Breaker)
            var sortedOffers = modelGroup
                .OrderBy(m => m.FinalPrice)
                .ThenBy(m => m.UpstreamId)
                .ThenBy(m => m.UpstreamGroupId)
                .ToList();
            for (int rank = 0; rank < sortedOffers.Count; rank++)
            {
                var offer = sortedOffers[rank];

                // --- 核心修改点 ---
                // 业务需求：价格越低，Priority 越高（数值越大）。
                // Rank 0 (最低价) -> Priority 100
                // Rank 1 (次低价) -> Priority 99
                int priority = MaxPriority - rank;

                // 防御性检查：防止 Priority 变成负数（假设虽然不太可能同一个模型有100个渠道）
                if (priority < 1) priority = 1;
                var key = new ChannelKey(offer.UpstreamId, offer.UpstreamGroupId, priority);
                if (!channelBuckets.ContainsKey(key))
                {
                    channelBuckets[key] = new List<ModelSync>();

                    var metaKey = (offer.UpstreamId, offer.UpstreamGroupId);
                    if (!channelMetaCache.ContainsKey(metaKey))
                    {
                        channelMetaCache[metaKey] = (
                            offer.Upstream!.Url,
                            offer.UpstreamGroup!.Key,
                            offer.Upstream.Name,
                            offer.UpstreamGroup.Name
                        );
                    }
                }

                channelBuckets[key].Add(offer);
            }
        }
        var result = new List<AddChannelRequest>();
        foreach (var kvp in channelBuckets)
        {
            var key = kvp.Key;
            var models = kvp.Value;
            var meta = channelMetaCache[(key.UpstreamId, key.GroupId)];
            var mapping = new Dictionary<string,string>();
            var request = new AddChannelRequest
            {
                // 建议名称带上 P{数值} 以便调试
                Name = $"{meta.GName}-P{key.Priority}",
                Type = 1,
                Key = meta.Key,
                BaseUrl = meta.BaseUrl,
                Models = string.Join(",", models.Select(m => m.Name)),
                ModelMapping = JsonSerializer.Serialize(mapping),
                Groups = new List<string> { "default" },
                Priority = key.Priority,
                Weight = 1
            };
            result.Add(request);
        }
        // 输出排序：优先显示 Priority 大的（即便宜的）
        return result.OrderByDescending(r => r.Priority).ThenBy(r => r.Name);
    }


    public Dictionary<string, string> BuildModelMappingForGroupWithFiltering(IEnumerable<string> modelNames)
    {
        var candidates = new Dictionary<string, (string name, DateTime date)>();
        var existing = new HashSet<string>(modelNames);

        (string baseName, DateTime date)? TryParse(string s, string fmt, int len)
        {
            if (s.Length > len && s[s.Length - len - 1] == '-'
                && DateTime.TryParseExact(s[^len..], fmt, null, System.Globalization.DateTimeStyles.None, out var d))
                return (s[..^(len + 1)], d);
            return null;
        }

        foreach (var name in modelNames)
        {
            var res = TryParse(name, "yyyy-MM-dd", 10) ?? TryParse(name, "yyyyMMdd", 8);
            if (res != null && !existing.Contains(res.Value.baseName))
                if (!candidates.TryGetValue(res.Value.baseName, out var cur) || res.Value.date > cur.date)
                    candidates[res.Value.baseName] = (name, res.Value.date);
        }

        return candidates.ToDictionary(x => x.Key, x => x.Value.name);
    }
}