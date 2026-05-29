using HCRS.Domain.Entities;
using Medo;
using Microsoft.EntityFrameworkCore;

namespace HCRS.Infrastructure.Persistence.Seed;

public static class AppDbSeeder
{
    public static async Task SeedAsync(HcrsDbContext context)
    {
        var systemId = Uuid7.NewUuid7();
        if (!await context.Roles.AnyAsync())
        {
            var adminRole = new UserRole
            {
                Id = Uuid7.NewUuid7(),
                Name = "Admin",
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = systemId,
            };

            var ownerRole = new UserRole
            {
                Id = Uuid7.NewUuid7(),
                Name = "Owner",
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = systemId,
            };

            var customerRole = new UserRole
            {
                Id = Uuid7.NewUuid7(),
                Name = "Customer",
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = systemId,
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
                    Id = Uuid7.NewUuid7(),
                    UserName = "admin",
                    UserEmail = "admin@hcrs.com",
                    UserDisplayName = "Admin",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"), 
                    RoleId = adminRole.Id,
                    CreatedAt = DateTimeOffset.UtcNow,
                    CreatedBy = systemId
                };

                await context.Users.AddAsync(adminUser);

                await context.SaveChangesAsync();
            }
        }
    }
}