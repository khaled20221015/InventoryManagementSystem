using InventoryManagementSystem.DataAccess.Identity;
using InventoryManagementSystem.DataAccess.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace InventoryManagementSystem.DataAccess.Data
{
    public static class DbSeeder
    {
        public const string AdminEmail = "kha20221015@std.psut.edu.jo";
        public const string AdminPassword = "Admin@123";

        public static async Task SeedAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();

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

            await SeedCannedGoodsAsync(context);
        }

        private static async Task SeedCannedGoodsAsync(ApplicationDbContext context)
        {
            const string CategoryName = "Canned Goods";

            if (await context.Categories.AnyAsync(c => c.Name == CategoryName))
            {
                return;
            }

            var category = new Category
            {
                Name = CategoryName,
                Description = "Tinned food with a shelf life."
            };

            await context.Categories.AddAsync(category);
            await context.SaveChangesAsync();

            var today = DateTime.Now.Date;

            var products = new List<Product>
            {
                NewCanned(category.Id, "Canned Sardines in Tomato Sauce 125g", 4.75m, 60, 30, today.AddDays(-40)),
                NewCanned(category.Id, "Canned Tuna in Olive Oil 185g", 9.50m, 24, 30, today.AddDays(-9)),
                NewCanned(category.Id, "Canned Fava Beans 400g", 3.25m, 90, 40, today),
                NewCanned(category.Id, "Canned Green Peas 400g", 3.75m, 55, 30, today.AddDays(2)),
                NewCanned(category.Id, "Canned Chickpeas 400g", 3.50m, 75, 40, today.AddDays(5)),
                NewCanned(category.Id, "Tomato Paste 380g", 5.00m, 120, 50, today.AddDays(19)),
                NewCanned(category.Id, "Canned Sweet Corn 340g", 4.25m, 65, 30, today.AddDays(27)),
                NewCanned(category.Id, "Evaporated Milk 410g", 6.75m, 48, 25, today.AddDays(210)),
                NewCanned(category.Id, "Canned Mushroom Slices 400g", 7.25m, 18, 20, today.AddDays(320))
            };

            await context.Products.AddRangeAsync(products);
            await context.SaveChangesAsync();
        }

        private static Product NewCanned(
            int categoryId, string name, decimal price, int stock, int minimum, DateTime expiry)
        {
            return new Product
            {
                Name = name,
                Description = "Demo stock for the expiry dashboard.",
                Price = price,
                StockQuantity = stock,
                MinimumStockLevel = minimum,
                ExpiryDate = expiry,
                CategoryId = categoryId
            };
        }
    }
}
