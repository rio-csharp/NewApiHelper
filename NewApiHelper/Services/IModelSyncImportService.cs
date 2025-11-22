using NewApiHelper.Models;

namespace NewApiHelper.Services;

public interface IModelSyncImportService
{
    Task ImportAsync(Upstream upstream, IEnumerable<UpstreamGroup> upstreamGroups);
}