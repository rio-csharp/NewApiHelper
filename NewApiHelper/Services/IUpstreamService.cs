using NewApiHelper.Models;

namespace NewApiHelper.Services;

public interface IUpstreamService
{
    Task<List<Upstream>> GetAllAsync();

    Task<Upstream?> GetByIdAsync(int id);

    Task<Upstream> AddAsync(Upstream channel);

    Task<Upstream> UpdateAsync(Upstream channel);

    Task DeleteAsync(int id);
}