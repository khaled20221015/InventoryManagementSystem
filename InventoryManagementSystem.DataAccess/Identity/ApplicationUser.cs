using InventoryManagementSystem.DataAccess.Models;
using Microsoft.AspNetCore.Identity;

namespace InventoryManagementSystem.DataAccess.Identity
{
    // The built-in Identity user plus the one extra field we need.
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;

        public List<StockTransaction> StockTransactions { get; set; } = new();
    }
}
