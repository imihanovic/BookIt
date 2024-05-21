using Microsoft.AspNetCore.Identity;
using BookIt.Core.Enums;
using BookIt.Core.Entities.Identity;
using BookIt.Core.Entities;
using BookIt.DataAccess.Repositories;

namespace BookIt.DataAccess.Persistence;

public static class DatabaseContextSeed
{
    public static async Task SeedDatabaseAsync(DatabaseContext context, RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager)
    {
        if (!roleManager.Roles.Any())
        {
            foreach(var name in Enum.GetNames(typeof(UserRole))) 
            {
                await roleManager.CreateAsync(new IdentityRole(name));
            }
        }
        if (!userManager.Users.Any())
        {
            var user = new ApplicationUser { UserName = "admin@admin.com", Email = "admin@admin.com", EmailConfirmed = true, FirstName="admin", LastName="admin"};

            await userManager.CreateAsync(user, "Admin123.?");
            await userManager.AddToRoleAsync(user, "Administrator");
        }
        await context.SaveChangesAsync();
        var user1 = new ApplicationUser { UserName = "pvidovic@gmail.com", Email = "pvidovic@gmail.com", PhoneNumber="09182748172" ,EmailConfirmed = true, FirstName = "Petar", LastName = "Vidovic" };
        await userManager.CreateAsync(user1, "Petar123.?");
        await userManager.AddToRoleAsync(user1, "Manager");
        var user2 = new ApplicationUser { UserName = "ikomadina@gmail.com", Email = "ikomadina@gmail.com", PhoneNumber = "09182112232", EmailConfirmed = true, FirstName = "Ivan", LastName = "Komadina" };
        await userManager.CreateAsync(user2, "Ivan123.?");
        await userManager.AddToRoleAsync(user2, "Manager");
        var user3 = new ApplicationUser { UserName = "imihanovic@gmail.com", Email = "imihanovic@gmail.com", PhoneNumber = "0914562232", EmailConfirmed = true, FirstName = "Ivana", LastName = "Mihanovic" };
        await userManager.CreateAsync(user3, "Ivana123.?");
        await userManager.AddToRoleAsync(user3, "Manager");
    }
}
