using Microsoft.EntityFrameworkCore;

namespace NewApiHelper.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Models.UpStreamChannel> UpStreamChannels { get; set; }
}