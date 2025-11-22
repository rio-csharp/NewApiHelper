using NewApiHelper.Models;

namespace NewApiHelper.Services;

public interface IUpstreamGroupService
{
    Task<List<UpstreamGroup>> GetAllAsync();

    Task<UpstreamGroup?> GetByIdAsync(int id);

    Task<UpstreamGroup> AddAsync(UpstreamGroup group);

    Task<UpstreamGroup> UpdateAsync(UpstreamGroup group);

    Task DeleteAsync(int id);
}