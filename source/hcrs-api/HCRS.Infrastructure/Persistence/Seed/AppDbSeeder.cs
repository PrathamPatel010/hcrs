using HCRS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HCRS.Infrastructure.Persistence.Seed;

public static class AppDbSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        if (!await context.Roles.AnyAsync())
        {
            var adminRole = new UserRole
            {
                Id = Guid.NewGuid(),
                Name = "Admin",
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = Guid.NewGuid(),
            };

            var ownerRole = new UserRole
            {
                Id = Guid.NewGuid(),
                Name = "Owner",
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = Guid.NewGuid(),
            };

            var customerRole = new UserRole
            {
                Id = Guid.NewGuid(),
                Name = "Customer",
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = Guid.NewGuid(),
            };

            await context.Roles.AddRangeAsync(
                adminRole,
                ownerRole,
                customerRole);

            await context.SaveChangesAsync();

            if (!await context.Users.AnyAsync())
            {
                var adminUser = new AppUser
                {
                    Id = Guid.NewGuid(),
                    UserName = "admin",
                    UserEmail = "admin@hcrs.com",
                    PasswordHash = "Admin@123", // For now only
                    RoleId = adminRole.Id,
                    CreatedAt = DateTimeOffset.UtcNow,
                    CreatedBy = Guid.NewGuid()
                };

                await context.Users.AddAsync(adminUser);

                await context.SaveChangesAsync();
            }
        }
    }
}