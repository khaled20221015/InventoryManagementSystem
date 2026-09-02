using InventoryManagementSystem.DataAccess.Models;
using Microsoft.AspNetCore.Identity;

namespace InventoryManagementSystem.DataAccess.Identity
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;

        public bool IsDeleted { get; set; }

        public List<StockTransaction> StockTransactions { get; set; } = new();
    }
}
