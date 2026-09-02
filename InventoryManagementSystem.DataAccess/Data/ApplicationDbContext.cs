using InventoryManagementSystem.DataAccess.Identity;
using InventoryManagementSystem.DataAccess.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementSystem.DataAccess.Data
{
    // The database. IdentityDbContext already brings the AspNet* user and role tables.
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }

        public DbSet<Category> Categories { get; set; }

        public DbSet<StockTransaction> StockTransactions { get; set; }
    }
}
