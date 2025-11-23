using Microsoft.EntityFrameworkCore;
using NewApiHelper.Data;
using NewApiHelper.Models;
using NewApiHelper.Services;
using System.Net.Http;
using System.Text.Json;

namespace NewApiHelper.Tests.Services;

public class ModelSyncImportServiceTests
{
    private void TestModels(Upstream upstream, UpstreamGroup upstreamGroup, string jsonFile, string actualPriceFile, Func<ModelSyncImportService, string, Upstream, UpstreamGroup, IEnumerable<ModelSync>> getModelsFunc, bool checkCount = true)
    {
        var jsonContent = File.ReadAllText(jsonFile);
        var actualPricesJson = File.ReadAllText(actualPriceFile);
        var actualPricesDoc = JsonDocument.Parse(actualPricesJson);
        var actualPrices = new Dictionary<string, decimal>();
        foreach (var property in actualPricesDoc.RootElement.EnumerateObject())
        {
            if (property.Value.ValueKind == JsonValueKind.Number)
            {
                actualPrices[property.Name] = property.Value.GetDecimal();
            }
        }
        var service = new ModelSyncImportService(null!,null!, null!); // We don't need context or http for this test

        var models = getModelsFunc(service, jsonContent, upstream, upstreamGroup);

        if (checkCount)
        {
            var modelsNames = models.Select(m => m.Name).ToHashSet();
            var actualNames = actualPrices.Keys.ToHashSet();
            var missingInModels = actualNames.Except(modelsNames).ToList();
            var extraInModels = modelsNames.Except(actualNames).ToList();
            if (missingInModels.Any() || extraInModels.Any())
            {
                var message = "Model count mismatch.\n";
                if (missingInModels.Any())
                {
                    message += $"Missing in models: {string.Join(", ", missingInModels)}\n";
                }
                if (extraInModels.Any())
                {
                    message += $"Extra in models: {string.Join(", ", extraInModels)}\n";
                }
                Assert.Fail(message);
            }
            Assert.Equal(actualPrices.Count(), models.Count());
        }
        foreach (var model in models)
        {
            Assert.NotNull(model.Name);
            Assert.Equal(upstream.Id, model.UpstreamId);
            Assert.Equal(upstreamGroup.Id, model.UpstreamGroupId);
            if (actualPrices.ContainsKey(model.Name) && model.QuotaType != QuotaType.NotSupported)
            {
                var expected = actualPrices[model.Name];
                var actual = model.FinalPrice;
                Assert.True(Math.Abs(actual - expected) < 0.01m, $"Model {model.Name}: expected {expected}, actual {actual}");
            }
        }
    }

    [Fact]
    public void GetModelsFromEZ_ReturnsCorrectModels()
    {
        var upstream = new Upstream { Id = 1, Name = "ez", Url = "http://example.com", UpstreamRatio = 1.0 };
        var upstreamGroup = new UpstreamGroup { Id = 1, GroupName = "default", GroupRatio = 1.0 };
        TestModels(upstream, upstreamGroup, "../../../Samples/EZ-Price.json", "../../../Samples/EZ-Actual-Price.json", (s, j, u, g) => s.GetModelsFromEZ(j, u, g));
    }

    [Fact]
    public void GetModelsFromQF_ReturnsCorrectModelsForDefault()
    {
        var upstream = new Upstream { Id = 2, Name = "qf", Url = "http://example.com", UpstreamRatio = 1.0 };
        var upstreamGroup = new UpstreamGroup { Id = 2, GroupName = "default", GroupRatio = 0.5 };
        TestModels(upstream, upstreamGroup, "../../../Samples/QF-Price.json", "../../../Samples/QF-Actual-Default-Price.json", (s, j, u, g) => s.GetModelsFromQF(j, u, g));
    }

    [Fact]
    public void GetModelsFromQF_ReturnsCorrectModelsForAzure()
    {
        var upstream = new Upstream { Id = 2, Name = "qf", Url = "http://example.com", UpstreamRatio = 1.0 };
        var upstreamGroup = new UpstreamGroup { Id = 2, GroupName = "Azure", GroupRatio = 0.5 };
        TestModels(upstream, upstreamGroup, "../../../Samples/QF-Price.json", "../../../Samples/QF-Actual-Azure-Price.json", (s, j, u, g) => s.GetModelsFromQF(j, u, g));
    }

    [Fact]
    public void GetModelsFromQF_ReturnsCorrectModelsForClaude()
    {
        var upstream = new Upstream { Id = 2, Name = "qf", Url = "http://example.com", UpstreamRatio = 1.0 };
        var upstreamGroup = new UpstreamGroup { Id = 2, GroupName = "Claude", GroupRatio = 0.4 };
        TestModels(upstream, upstreamGroup, "../../../Samples/QF-Price.json", "../../../Samples/QF-Actual-Claude-Price.json", (s, j, u, g) => s.GetModelsFromQF(j, u, g));
    }

    [Fact]
    public void GetModelsFromQF_ReturnsCorrectModelsForGemini()
    {
        var upstream = new Upstream { Id = 2, Name = "qf", Url = "http://example.com", UpstreamRatio = 1.0 };
        var upstreamGroup = new UpstreamGroup { Id = 2, GroupName = "Gemini", GroupRatio = 0.3 };
        TestModels(upstream, upstreamGroup, "../../../Samples/QF-Price.json", "../../../Samples/QF-Actual-Gemini-Price.json", (s, j, u, g) => s.GetModelsFromQF(j, u, g));
    }

    [Fact]
    public void GetModelsFromQF_ReturnsCorrectModelsForKimi()
    {
        var upstream = new Upstream { Id = 2, Name = "qf", Url = "http://example.com", UpstreamRatio = 1.0 };
        var upstreamGroup = new UpstreamGroup { Id = 2, GroupName = "Kimi", GroupRatio = 0.5 };
        TestModels(upstream, upstreamGroup, "../../../Samples/QF-Price.json", "../../../Samples/QF-Actual-Kimi-Price.json", (s, j, u, g) => s.GetModelsFromQF(j, u, g));
    }

    [Fact]
    public void GetModelsFromQF_ReturnsCorrectModelsForAWSClaude1()
    {
        var upstream = new Upstream { Id = 2, Name = "qf", Url = "http://example.com", UpstreamRatio = 1.0 };
        var upstreamGroup = new UpstreamGroup { Id = 2, GroupName = "AWS Claude1", GroupRatio = 2.0 };
        TestModels(upstream, upstreamGroup, "../../../Samples/QF-Price.json", "../../../Samples/QF-Actual-AWS Claude1-Price.json", (s, j, u, g) => s.GetModelsFromQF(j, u, g));
    }

    [Fact]
    public void GetModelsFromQF_ReturnsCorrectModelsForClaudeCode()
    {
        var upstream = new Upstream { Id = 2, Name = "qf", Url = "http://example.com", UpstreamRatio = 1.0 };
        var upstreamGroup = new UpstreamGroup { Id = 2, GroupName = "Claude Code", GroupRatio = 0.8 };
        TestModels(upstream, upstreamGroup, "../../../Samples/QF-Price.json", "../../../Samples/QF-Actual-ClaudeCode-Price.json", (s, j, u, g) => s.GetModelsFromQF(j, u, g));
    }

    [Fact]
    public void GetModelsFromQF_ReturnsCorrectModelsForDeepSeek1()
    {
        var upstream = new Upstream { Id = 2, Name = "qf", Url = "http://example.com", UpstreamRatio = 1.0 };
        var upstreamGroup = new UpstreamGroup { Id = 2, GroupName = "DeepSeek1", GroupRatio = 0.5 };
        TestModels(upstream, upstreamGroup, "../../../Samples/QF-Price.json", "../../../Samples/QF-Actual-DeepSeek1-Price.json", (s, j, u, g) => s.GetModelsFromQF(j, u, g));
    }

    [Fact]
    public void GetModelsFromGG_ReturnsCorrectModels()
    {
        var upstream = new Upstream { Id = 1, Name = "gg", Url = "http://example.com", UpstreamRatio = 1.0 };
        var upstreamGroup = new UpstreamGroup { Id = 1, GroupName = "default", GroupRatio = 1.0 };
        TestModels(upstream, upstreamGroup, "../../../Samples/GG-Price.json", "../../../Samples/GG-Actual-Price.json", (s, j, u, g) => s.GetModelsFromGG(j, u, g),  false);
    }

    [Fact]
    public void GetModelsFromVC_ReturnsCorrectModelsForClaudeCode专属()
    {
        var upstream = new Upstream { Id = 3, Name = "vc", Url = "http://example.com", UpstreamRatio = 1.0 };
        var upstreamGroup = new UpstreamGroup { Id = 3, GroupName = "Claude Code专属", GroupRatio = 1.6 };
        TestModels(upstream, upstreamGroup, "../../../Samples/VC-Price.json", "../../../Samples/VC-Actual-ClaudeCode专属-Price.json", (s, j, u, g) => s.GetModelsFromVC(j, u, g));
    }

    [Fact]
    public void GetModelsFromVC_ReturnsCorrectModelsForCodex专属()
    {
        var upstream = new Upstream { Id = 3, Name = "vc", Url = "http://example.com", UpstreamRatio = 1.0 };
        var upstreamGroup = new UpstreamGroup { Id = 4, GroupName = "Codex专属", GroupRatio = 0.6 };
        TestModels(upstream, upstreamGroup, "../../../Samples/VC-Price.json", "../../../Samples/VC-Actual-Codex专属-Price.json", (s, j, u, g) => s.GetModelsFromVC(j, u, g));
    }

    [Fact]
    public void GetModelsFromVC_ReturnsCorrectModelsForDefault()
    {
        var upstream = new Upstream { Id = 3, Name = "vc", Url = "http://example.com", UpstreamRatio = 1.0 };
        var upstreamGroup = new UpstreamGroup { Id = 5, GroupName = "default", GroupRatio = 1.0 };
        TestModels(upstream, upstreamGroup, "../../../Samples/VC-Price.json", "../../../Samples/VC-Actual-default-Price.json", (s, j, u, g) => s.GetModelsFromVC(j, u, g));
    }

    [Fact]
    public void GetModelsFromVC_ReturnsCorrectModelsForMJ慢速()
    {
        var upstream = new Upstream { Id = 3, Name = "vc", Url = "http://example.com", UpstreamRatio = 1.0 };
        var upstreamGroup = new UpstreamGroup { Id = 6, GroupName = "MJ慢速", GroupRatio = 0.5 };
        TestModels(upstream, upstreamGroup, "../../../Samples/VC-Price.json", "../../../Samples/VC-Actual-MJ慢速-Price.json", (s, j, u, g) => s.GetModelsFromVC(j, u, g));
    }

    [Fact]
    public void GetModelsFromVC_ReturnsCorrectModelsForOfficialClaude()
    {
        var upstream = new Upstream { Id = 3, Name = "vc", Url = "http://example.com", UpstreamRatio = 1.0 };
        var upstreamGroup = new UpstreamGroup { Id = 7, GroupName = "official_Claude", GroupRatio = 8.0 };
        TestModels(upstream, upstreamGroup, "../../../Samples/VC-Price.json", "../../../Samples/VC-Actual-official_Claude-Price.json", (s, j, u, g) => s.GetModelsFromVC(j, u, g));
    }

    [Fact]
    public void GetModelsFromVC_ReturnsCorrectModelsFor优质gemini()
    {
        var upstream = new Upstream { Id = 3, Name = "vc", Url = "http://example.com", UpstreamRatio = 1.0 };
        var upstreamGroup = new UpstreamGroup { Id = 8, GroupName = "优质gemini", GroupRatio = 1.0 };
        TestModels(upstream, upstreamGroup, "../../../Samples/VC-Price.json", "../../../Samples/VC-Actual-优质gemini-Price.json", (s, j, u, g) => s.GetModelsFromVC(j, u, g));
    }

    [Fact]
    public void GetModelsFromVC_ReturnsCorrectModelsFor优质官转OpenAI()
    {
        var upstream = new Upstream { Id = 3, Name = "vc", Url = "http://example.com", UpstreamRatio = 1.0 };
        var upstreamGroup = new UpstreamGroup { Id = 9, GroupName = "优质官转OpenAI", GroupRatio = 8.0 };
        TestModels(upstream, upstreamGroup, "../../../Samples/VC-Price.json", "../../../Samples/VC-Actual-优质官转OpenAI-Price.json", (s, j, u, g) => s.GetModelsFromVC(j, u, g));
    }

    [Fact]
    public void GetModelsFromVC_ReturnsCorrectModelsFor官转()
    {
        var upstream = new Upstream { Id = 3, Name = "vc", Url = "http://example.com", UpstreamRatio = 1.0 };
        var upstreamGroup = new UpstreamGroup { Id = 10, GroupName = "官转", GroupRatio = 3.0 };
        TestModels(upstream, upstreamGroup, "../../../Samples/VC-Price.json", "../../../Samples/VC-Actual-官转-Price.json", (s, j, u, g) => s.GetModelsFromVC(j, u, g));
    }

    [Fact]
    public void GetModelsFromVC_ReturnsCorrectModelsFor官转gemini()
    {
        var upstream = new Upstream { Id = 3, Name = "vc", Url = "http://example.com", UpstreamRatio = 1.0 };
        var upstreamGroup = new UpstreamGroup { Id = 11, GroupName = "官转gemini", GroupRatio = 3.0 };
        TestModels(upstream, upstreamGroup, "../../../Samples/VC-Price.json", "../../../Samples/VC-Actual-官转gemini-Price.json", (s, j, u, g) => s.GetModelsFromVC(j, u, g));
    }

    [Fact]
    public void GetModelsFromVC_ReturnsCorrectModelsFor官转OpenAI()
    {
        var upstream = new Upstream { Id = 3, Name = "vc", Url = "http://example.com", UpstreamRatio = 1.0 };
        var upstreamGroup = new UpstreamGroup { Id = 12, GroupName = "官转OpenAI", GroupRatio = 6.0 };
        TestModels(upstream, upstreamGroup, "../../../Samples/VC-Price.json", "../../../Samples/VC-Actual-官转OpenAI-Price.json", (s, j, u, g) => s.GetModelsFromVC(j, u, g));
    }

    [Fact]
    public void GetModelsFromVC_ReturnsCorrectModelsFor官转克劳德2()
    {
        var upstream = new Upstream { Id = 3, Name = "vc", Url = "http://example.com", UpstreamRatio = 1.0 };
        var upstreamGroup = new UpstreamGroup { Id = 13, GroupName = "官转克劳德2", GroupRatio = 6.0 };
        TestModels(upstream, upstreamGroup, "../../../Samples/VC-Price.json", "../../../Samples/VC-Actual-官转克劳德2-Price.json", (s, j, u, g) => s.GetModelsFromVC(j, u, g));
    }

    [Fact]
    public void GetModelsFromVC_ReturnsCorrectModelsFor官转克劳德3()
    {
        var upstream = new Upstream { Id = 3, Name = "vc", Url = "http://example.com", UpstreamRatio = 1.0 };
        var upstreamGroup = new UpstreamGroup { Id = 14, GroupName = "官转克劳德3", GroupRatio = 12.0 };
        TestModels(upstream, upstreamGroup, "../../../Samples/VC-Price.json", "../../../Samples/VC-Actual-官转克劳德3-Price.json", (s, j, u, g) => s.GetModelsFromVC(j, u, g));
    }

    [Fact]
    public void GetModelsFromVC_ReturnsCorrectModelsFor直连克劳德()
    {
        var upstream = new Upstream { Id = 3, Name = "vc", Url = "http://example.com", UpstreamRatio = 1.0 };
        var upstreamGroup = new UpstreamGroup { Id = 15, GroupName = "直连克劳德", GroupRatio = 16.0 };
        TestModels(upstream, upstreamGroup, "../../../Samples/VC-Price.json", "../../../Samples/VC-Actual-直连克劳德-Price.json", (s, j, u, g) => s.GetModelsFromVC(j, u, g));
    }

    [Fact]
    public void GetModelsFromVC_ReturnsCorrectModelsFor纯AZ()
    {
        var upstream = new Upstream { Id = 3, Name = "vc", Url = "http://example.com", UpstreamRatio = 1.0 };
        var upstreamGroup = new UpstreamGroup { Id = 16, GroupName = "纯AZ", GroupRatio = 1.5 };
        TestModels(upstream, upstreamGroup, "../../../Samples/VC-Price.json", "../../../Samples/VC-Actual-纯AZ-Price.json", (s, j, u, g) => s.GetModelsFromVC(j, u, g));
    }

    [Fact]
    public void GetModelsFromVC_ReturnsCorrectModelsFor逆向()
    {
        var upstream = new Upstream { Id = 3, Name = "vc", Url = "http://example.com", UpstreamRatio = 1.0 };
        var upstreamGroup = new UpstreamGroup { Id = 17, GroupName = "逆向", GroupRatio = 1.4 };
        TestModels(upstream, upstreamGroup, "../../../Samples/VC-Price.json", "../../../Samples/VC-Actual-逆向-Price.json", (s, j, u, g) => s.GetModelsFromVC(j, u, g));
    }

    [Fact]
    public void GetModelsFromVC_ReturnsCorrectModelsFor限时特价()
    {
        var upstream = new Upstream { Id = 3, Name = "vc", Url = "http://example.com", UpstreamRatio = 1.0 };
        var upstreamGroup = new UpstreamGroup { Id = 18, GroupName = "限时特价", GroupRatio = 0.6 };
        TestModels(upstream, upstreamGroup, "../../../Samples/VC-Price.json", "../../../Samples/VC-Actual-限时特价-Price.json", (s, j, u, g) => s.GetModelsFromVC(j, u, g));
    }

    [Fact]
    public async Task ImportAsync_ValidData_ImportsModels()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new AppDbContext(options);
        var httpClient = new HttpClient(new MockHttpMessageHandler());

        var service = new ModelSyncImportService(context, httpClient);

        var upstream = new Upstream
        {
            Id = 1,
            Name = "ez",
            Url = "http://test.com",
            UpstreamRatio = 1.0
        };

        var upstreamGroups = new List<UpstreamGroup>
        {
            new UpstreamGroup { Id = 1, GroupName = "default", GroupRatio = 1.0 }
        };

        // Act
        await service.ImportAsync(upstream, upstreamGroups);

        // Assert
        var models = await context.ModelSyncs.ToListAsync();
        Assert.NotEmpty(models);
    }
}

// Mock HttpMessageHandler for testing
public class MockHttpMessageHandler : HttpMessageHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var jsonResponse = @"{
            ""data"": [
                {
                    ""model_name"": ""test-model"",
                    ""quota_type"": 0,
                    ""model_ratio"": 1.0,
                    ""model_price"": 1.0,
                    ""completion_ratio"": 1.0
                }
            ]
        }";
        var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
        {
            Content = new StringContent(jsonResponse)
        };
        return response;
    }
}