using Microsoft.AspNetCore.Identity;
using InventoryManagementSystem.DataAccess.Models;
using System.Collections.Generic;

namespace InventoryManagementSystem.DataAccess.Identity
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;

        public List<StockTransaction> StockTransactions { get; set; } = new List<StockTransaction>();
    }
}