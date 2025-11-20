using NewApiHelper.Models;
using System.Net.Http;
using System.Net.Http.Json;
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
}