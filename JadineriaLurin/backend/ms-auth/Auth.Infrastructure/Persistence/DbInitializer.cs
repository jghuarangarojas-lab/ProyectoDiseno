using Auth.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Auth.Infrastructure.Persistence;

public static class DbInitializer
{
    public static async Task SeedAsync(AuthDbContext context)
    {
        if (await context.Roles.AnyAsync())
        {
            return;
        }

        foreach (var name in Role.Names.All)
        {
            context.Roles.Add(new Role(name));
        }

        await context.SaveChangesAsync();
    }
}
