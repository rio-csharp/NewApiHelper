using NewApiHelper.Models;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;

namespace NewApiHelper.Services;

public interface ITestService
{
    Task<TestResult> TestModelAsync(Upstream upstream, UpstreamGroup upstreamGroup, string modelName);
}

public class TestResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}

public class TestService : ITestService
{
    private readonly HttpClient _httpClient;

    public TestService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<TestResult> TestModelAsync(Upstream upstream, UpstreamGroup upstreamGroup, string modelName)
    {
        try
        {
            var url = $"{upstream.Url.TrimEnd('/')}/v1/chat/completions";
            var request = new
            {
                model = modelName,
                messages = new[]
                {
                    new { role = "user", content = "Hello" }
                },
                max_tokens = 10
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", upstreamGroup.Key);

            var response = await _httpClient.PostAsync(url, content);

            if (response.IsSuccessStatusCode)
            {
                return new TestResult { Success = true };
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                return new TestResult { Success = false, ErrorMessage = $"HTTP {response.StatusCode}: {errorContent}" };
            }
        }
        catch (Exception ex)
        {
            return new TestResult { Success = false, ErrorMessage = ex.Message };
        }
    }
}