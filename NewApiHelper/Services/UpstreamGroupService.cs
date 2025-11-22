using Microsoft.EntityFrameworkCore;
using NewApiHelper.Data;
using NewApiHelper.Models;

namespace NewApiHelper.Services;

public class UpstreamGroupService : IUpstreamGroupService
{
    private readonly AppDbContext _context;

    public UpstreamGroupService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<UpstreamGroup>> GetAllAsync()
    {
        return await _context.UpstreamGroups.Include(g => g.Upstream).ToListAsync();
    }

    public async Task<UpstreamGroup?> GetByIdAsync(int id)
    {
        return await _context.UpstreamGroups.Include(g => g.Upstream).FirstOrDefaultAsync(g => g.Id == id);
    }

    public async Task<UpstreamGroup> AddAsync(UpstreamGroup group)
    {
        _context.UpstreamGroups.Add(group);
        await _context.SaveChangesAsync();
        return group;
    }

    public async Task<UpstreamGroup> UpdateAsync(UpstreamGroup group)
    {
        _context.UpstreamGroups.Update(group);
        await _context.SaveChangesAsync();
        return group;
    }

    public async Task DeleteAsync(int id)
    {
        var group = await _context.UpstreamGroups.FindAsync(id);
        if (group != null)
        {
            _context.UpstreamGroups.Remove(group);
            await _context.SaveChangesAsync();
        }
    }
}