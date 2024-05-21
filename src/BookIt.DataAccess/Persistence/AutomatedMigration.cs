using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using BookIt.Core.Entities.Identity;
using BookIt.DataAccess.Repositories;

namespace BookIt.DataAccess.Persistence;

public static class AutomatedMigration
{
    public static async Task MigrateAsync(IServiceProvider services)
    {
        var context = services.GetRequiredService<DatabaseContext>();

        if (context.Database.IsNpgsql()) await context.Database.MigrateAsync();

        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        var roleManager=services.GetRequiredService<RoleManager<IdentityRole>>();

        await DatabaseContextSeed.SeedDatabaseAsync(context, roleManager, userManager);
    }
}
