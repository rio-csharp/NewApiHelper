using NewApiHelper.Models;

namespace NewApiHelper.Services;

public interface IUpStreamChannelService
{
    Task<List<UpStreamChannel>> GetAllAsync();
    Task<UpStreamChannel?> GetByIdAsync(int id);
    Task<UpStreamChannel> AddAsync(UpStreamChannel channel);
    Task<UpStreamChannel> UpdateAsync(UpStreamChannel channel);
    Task DeleteAsync(int id);
}