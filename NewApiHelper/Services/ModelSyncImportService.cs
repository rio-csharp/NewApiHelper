using Microsoft.EntityFrameworkCore;
using NewApiHelper.Data;
using NewApiHelper.Models;
using System.Net.Http;
using System.Text.Json.Nodes;

namespace NewApiHelper.Services;

public class ModelSyncImportService : IModelSyncImportService
{
    private readonly AppDbContext _context;
    private readonly HttpClient _httpClient;

    public ModelSyncImportService(AppDbContext context, HttpClient httpClient)
    {
        _context = context;
        _httpClient = httpClient;
    }

    public async Task ImportAsync(Upstream upstream, IEnumerable<UpstreamGroup> upstreamGroups)
    {
        var json = await GetPriceJson(upstream);
        foreach (var group in upstreamGroups)
        {
            var models = GetModels(json, upstream, group);
            await UpsertModels(models);
        }
    }

    private async Task<string> GetPriceJson(Upstream upstream)
    {
        var url = upstream.Url + "/api/pricing";
        var response = await _httpClient.GetStringAsync(url);
        return response;
    }

    public IEnumerable<ModelSync> GetModels(string jsonContent, Upstream upstream, UpstreamGroup upstreamGroup)
    {
        if (jsonContent == null)
            return Enumerable.Empty<ModelSync>();

        return upstream.Name.ToLower() switch
        {
            "ez" => GetModelsFromEZ(jsonContent, upstream, upstreamGroup),
            "qf" => GetModelsFromQF(jsonContent, upstream, upstreamGroup),
            "gg" => GetModelsFromGG(jsonContent, upstream, upstreamGroup),
            "vc" => GetModelsFromVC(jsonContent, upstream, upstreamGroup),
            _ => Enumerable.Empty<ModelSync>()
        };
    }

    public IEnumerable<ModelSync> GetModelsFromEZ(string jsonContent, Upstream upstream, UpstreamGroup upstreamGroup)
    {
        var root = JsonNode.Parse(jsonContent);
        if (root is null) return Enumerable.Empty<ModelSync>();
        var dataNode = root["data"];
        if (dataNode is null) return Enumerable.Empty<ModelSync>();
        var data = dataNode.AsArray();

        var models = new List<ModelSync>();

        foreach (var item in data)
        {
            if (item is null) continue;
            var nameNode = item["model_name"];
            if (nameNode is null) continue;
            var name = nameNode.GetValue<string>();

            var ratioNode = item["model_ratio"];
            var completionRatioNode = item["completion_ratio"];
            var quotaTypeNode = item["quota_type"];
            var priceNode = item["model_price"];
            if (ratioNode is null || completionRatioNode is null || quotaTypeNode is null || priceNode is null) continue;

            var model = new ModelSync
            {
                Name = name,
                Ratio = ratioNode.GetValue<double>(),
                Price = priceNode.GetValue<decimal>(),
                CompletionRatio = completionRatioNode.GetValue<double>(),
                QuotaType = (QuotaType)quotaTypeNode.GetValue<int>(),
                UpstreamId = upstream.Id,
                UpstreamGroupId = upstreamGroup.Id,
                Upstream = upstream,
                UpstreamGroup = upstreamGroup
            };
            models.Add(model);
        }

        return models;
    }

    public IEnumerable<ModelSync> GetModelsFromQF(string jsonContent, Upstream upstream, UpstreamGroup upstreamGroup)
    {
        var root = JsonNode.Parse(jsonContent);
        if (root is null) return Enumerable.Empty<ModelSync>();
        var dataNode = root["data"];
        if (dataNode is null) return Enumerable.Empty<ModelSync>();
        var modelInfoNode = dataNode["model_info"];
        if (modelInfoNode is null) return Enumerable.Empty<ModelSync>();
        var modelInfo = modelInfoNode.AsArray();

        var models = new List<ModelSync>();

        foreach (var modelNode in modelInfo)
        {
            if (modelNode is null) continue;
            var nameNode = modelNode["model_name"];
            if (nameNode is null) continue;
            var name = nameNode.GetValue<string>();
            var priceInfoNode = modelNode["price_info"];
            if (priceInfoNode is null) continue;
            var priceInfo = priceInfoNode.AsObject();
            if (!priceInfo.ContainsKey(upstreamGroup.Name)) continue;
            var groupPriceNode = priceInfo[upstreamGroup.Name];
            if (groupPriceNode is null) continue;
            var defaultPriceNode = groupPriceNode["default"];
            if (defaultPriceNode is null) continue;

            var quotaTypeNode = defaultPriceNode["quota_type"];
            var modelPriceNode = defaultPriceNode["model_price"];
            var modelRatioNode = defaultPriceNode["model_ratio"];
            var completionRatioNode = defaultPriceNode["model_completion_ratio"];
            if (quotaTypeNode is null || modelPriceNode is null || modelRatioNode is null || completionRatioNode is null) continue;

            var quotaType = quotaTypeNode.GetValue<int>();
            var modelPrice = modelPriceNode.GetValue<decimal>();
            var modelRatio = modelRatioNode.GetValue<double>();
            var completionRatio = completionRatioNode.GetValue<double>();

            var model = new ModelSync
            {
                Name = name,
                Ratio = modelRatio,
                Price = modelPrice,
                CompletionRatio = completionRatio,
                QuotaType = quotaType == 1 ? QuotaType.PayAsYouGo : QuotaType.PayPerUse,
                UpstreamId = upstream.Id,
                UpstreamGroupId = upstreamGroup.Id,
                Upstream = upstream,
                UpstreamGroup = upstreamGroup
            };
            models.Add(model);
        }

        return models;
    }

    public IEnumerable<ModelSync> GetModelsFromGG(string jsonContent, Upstream upstream, UpstreamGroup upstreamGroup)
    {
        var root = JsonNode.Parse(jsonContent);
        if (root is null) return Enumerable.Empty<ModelSync>();
        var dataNode = root["data"];
        if (dataNode is null) return Enumerable.Empty<ModelSync>();
        var modelRatioNode = dataNode["ModelRatio"];
        var completionRatioNode = dataNode["CompletionRatio"];
        var modelFixedPriceNode = dataNode["ModelFixedPrice"];
        if (modelRatioNode is null || completionRatioNode is null || modelFixedPriceNode is null) return Enumerable.Empty<ModelSync>();

        var ratioObj = modelRatioNode.AsObject();
        var completionObj = completionRatioNode.AsObject();
        var priceObj = modelFixedPriceNode.AsObject();
        if (ratioObj is null || completionObj is null || priceObj is null) return Enumerable.Empty<ModelSync>();

        var ratioMap = ratioObj.Where(kvp => kvp.Value != null).ToDictionary(kvp => kvp.Key, kvp => kvp.Value!.GetValue<double>());
        var completionMap = completionObj.Where(kvp => kvp.Value != null).ToDictionary(kvp => kvp.Key, kvp => kvp.Value!.GetValue<double>());
        var priceMap = priceObj.Where(kvp => kvp.Value != null).ToDictionary(kvp => kvp.Key, kvp => kvp.Value!.GetValue<decimal>());

        var models = new List<ModelSync>();

        foreach (var name in completionMap.Keys)
        {
            var ratio = ratioMap.ContainsKey(name) ? ratioMap[name] : 1.0;
            var completionRatio = completionMap[name];
            var price = 0m;

            var model = new ModelSync
            {
                Name = name,
                Ratio = ratio,
                Price = price,
                CompletionRatio = completionRatio,
                QuotaType = QuotaType.PayAsYouGo,
                UpstreamId = upstream.Id,
                UpstreamGroupId = upstreamGroup.Id,
                Upstream = upstream,
                UpstreamGroup = upstreamGroup
            };
            models.Add(model);
        }

        return models;
    }

    public IEnumerable<ModelSync> GetModelsFromVC(string jsonContent, Upstream upstream, UpstreamGroup upstreamGroup)
    {
        var root = JsonNode.Parse(jsonContent);
        if (root is null) return Enumerable.Empty<ModelSync>();
        var dataNode = root["data"];
        if (dataNode is null) return Enumerable.Empty<ModelSync>();
        var modelCompletionRatioNode = dataNode["model_completion_ratio"];
        var modelGroupNode = dataNode["model_group"];
        if (modelGroupNode is null) return Enumerable.Empty<ModelSync>();
        var groupNode = modelGroupNode[upstreamGroup.Name];
        if (groupNode is null) return Enumerable.Empty<ModelSync>();
        var groupRatioNode = groupNode["GroupRatio"];
        if (groupRatioNode is null) return Enumerable.Empty<ModelSync>();
        var groupRatio = groupRatioNode.GetValue<double>();
        var modelPriceNode = groupNode["ModelPrice"];
        if (modelPriceNode is null) return Enumerable.Empty<ModelSync>();
        var modelPrice = modelPriceNode.AsObject();
        if (modelPrice is null) return Enumerable.Empty<ModelSync>();

        var models = new List<ModelSync>();

        foreach (var kvp in modelPrice)
        {
            var modelName = kvp.Key;
            var priceInfo = kvp.Value;
            if (priceInfo is null) continue;
            var priceTypeNode = priceInfo["priceType"];
            if (priceTypeNode is null) continue;
            var priceType = priceTypeNode.GetValue<int>();
            QuotaType quotaType;
            if (priceType == 0)
            {
                quotaType = QuotaType.PayAsYouGo;
            }
            else if (priceType == 1)
            {
                quotaType = QuotaType.PayPerUse;
            }
            else
            {
                quotaType = QuotaType.NotSupported;
            }
            var priceNode = priceInfo["price"];
            if (priceNode is null) continue;
            var price = priceNode.GetValue<decimal>();
            var completionRatio = modelCompletionRatioNode?[modelName]?.GetValue<double>() ?? 1;

            var model = new ModelSync
            {
                Name = modelName,
                Ratio = priceType == 0 ? (double)price : 1,
                Price = priceType == 1 ? price : 0,
                CompletionRatio = completionRatio,
                QuotaType = quotaType,
                UpstreamId = upstream.Id,
                UpstreamGroupId = upstreamGroup.Id,
                Upstream = upstream,
                UpstreamGroup = upstreamGroup
            };
            models.Add(model);
        }

        return models;
    }

    private async Task UpsertModels(IEnumerable<ModelSync> models)
    {
        foreach (var model in models)
        {
            var existing = await _context.ModelSyncs.FirstOrDefaultAsync(m => m.Name == model.Name);
            if (existing != null)
            {
                // Update existing model without changing Id
                existing.Name = model.Name;
                existing.Ratio = model.Ratio;
                existing.Price = model.Price;
                existing.CompletionRatio = model.CompletionRatio;
                existing.QuotaType = model.QuotaType;
                existing.UpstreamId = model.UpstreamId;
                existing.UpstreamGroupId = model.UpstreamGroupId;
                existing.Upstream = model.Upstream;
                existing.UpstreamGroup = model.UpstreamGroup;
                // Do not update CreatedAt or Id
            }
            else
            {
                _context.ModelSyncs.Add(model);
            }
        }
        await _context.SaveChangesAsync();
    }
}