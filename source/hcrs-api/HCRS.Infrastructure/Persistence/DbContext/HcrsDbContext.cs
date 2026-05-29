using HCRS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

public class HcrsDbContext : DbContext
{
    public HcrsDbContext(DbContextOptions<HcrsDbContext> options) : base(options)
    {
    }

    public DbSet<UserRole> Roles => Set<UserRole>();
    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(HcrsDbContext).Assembly);
    }
}
