using InventoryManagementSystem.DataAccess.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace InventoryManagementSystem.DataAccess.Data
{
    // Prepares the database on every start: applies the migration, then creates
    // the two roles and the default admin account if they are not there yet.
    public static class DbSeeder
    {
        public const string AdminEmail = "admin@inventory.com";
        public const string AdminPassword = "Admin@123";

        public static async Task SeedAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();

            // Creates the database if it does not exist and applies the migration.
            // This is why the app also works on a fresh machine such as the IIS server.
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await context.Database.MigrateAsync();

            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            foreach (var role in new[] { RoleNames.Admin, RoleNames.Employee })
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            if (await userManager.FindByEmailAsync(AdminEmail) is null)
            {
                var admin = new ApplicationUser
                {
                    UserName = AdminEmail,
                    Email = AdminEmail,
                    FullName = "System Administrator",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(admin, AdminPassword);

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, RoleNames.Admin);
                }
            }
        }
    }
}
