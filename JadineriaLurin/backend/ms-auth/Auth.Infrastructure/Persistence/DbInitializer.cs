using Auth.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Auth.Infrastructure.Persistence;

public static class DbInitializer
{
    private const string AdminUsername = "admin@JLADMIN";
    private const string AdminEmail = "admin@jardinerialurin.pe";
    private const string AdminPassword = "Admin123!";

    public static async Task SeedAsync(AuthDbContext context)
    {
        if (!await context.Roles.AnyAsync())
        {
            foreach (var name in Role.Names.All)
            {
                context.Roles.Add(new Role(name));
            }

            await context.SaveChangesAsync();
        }

        if (await context.Users.AnyAsync(user => user.Username == AdminUsername))
        {
            return;
        }

        var adminRole = await context.Roles.SingleAsync(role => role.Name == Role.Names.Admin);

        var admin = User.Create(
            AdminUsername,
            BCrypt.Net.BCrypt.HashPassword(AdminPassword),
            AdminEmail,
            Role.Names.Admin);

        admin.AssignRole(adminRole);

        context.Users.Add(admin);
        await context.SaveChangesAsync();
    }
}
