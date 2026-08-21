using InventoryManagementSystem.DataAccess.Models;

namespace InventoryManagementSystem.Presentation.Models
{
    public class DashboardViewModel
    {
        public int TotalProducts { get; set; }

        public int TotalCategories { get; set; }

        public List<Product> LowStockProducts { get; set; } = new();
    }
}