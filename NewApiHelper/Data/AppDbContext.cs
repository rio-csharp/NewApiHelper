using Microsoft.EntityFrameworkCore;

namespace NewApiHelper.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Models.Upstream> UpStreams { get; set; }
    public DbSet<Models.UpstreamGroup> UpstreamGroups { get; set; }
    public DbSet<Models.ModelSync> ModelSyncs { get; set; }
}