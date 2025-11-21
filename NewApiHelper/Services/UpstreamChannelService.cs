using Microsoft.EntityFrameworkCore;
using NewApiHelper.Data;
using NewApiHelper.Models;

namespace NewApiHelper.Services;

public class UpStreamChannelService : IUpStreamChannelService
{
    private readonly AppDbContext _context;

    public UpStreamChannelService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<UpStreamChannel>> GetAllAsync()
    {
        return await _context.UpStreamChannels.ToListAsync();
    }

    public async Task<UpStreamChannel?> GetByIdAsync(int id)
    {
        return await _context.UpStreamChannels.FindAsync(id);
    }

    public async Task<UpStreamChannel> AddAsync(UpStreamChannel channel)
    {
        _context.UpStreamChannels.Add(channel);
        await _context.SaveChangesAsync();
        return channel;
    }

    public async Task<UpStreamChannel> UpdateAsync(UpStreamChannel channel)
    {
        _context.UpStreamChannels.Update(channel);
        await _context.SaveChangesAsync();
        return channel;
    }

    public async Task DeleteAsync(int id)
    {
        var channel = await _context.UpStreamChannels.FindAsync(id);
        if (channel != null)
        {
            _context.UpStreamChannels.Remove(channel);
            await _context.SaveChangesAsync();
        }
    }
}