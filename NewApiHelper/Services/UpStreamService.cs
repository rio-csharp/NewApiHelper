using Microsoft.EntityFrameworkCore;
using NewApiHelper.Data;
using NewApiHelper.Models;

namespace NewApiHelper.Services;

public class UpStreamService : IUpstreamService
{
    private readonly AppDbContext _context;

    public UpStreamService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Upstream>> GetAllAsync()
    {
        return await _context.UpStreams.ToListAsync();
    }

    public async Task<Upstream?> GetByIdAsync(int id)
    {
        return await _context.UpStreams.FindAsync(id);
    }

    public async Task<Upstream> AddAsync(Upstream channel)
    {
        _context.UpStreams.Add(channel);
        await _context.SaveChangesAsync();
        return channel;
    }

    public async Task<Upstream> UpdateAsync(Upstream channel)
    {
        _context.UpStreams.Update(channel);
        await _context.SaveChangesAsync();
        return channel;
    }

    public async Task DeleteAsync(int id)
    {
        var channel = await _context.UpStreams.FindAsync(id);
        if (channel != null)
        {
            _context.UpStreams.Remove(channel);
            await _context.SaveChangesAsync();
        }
    }
}