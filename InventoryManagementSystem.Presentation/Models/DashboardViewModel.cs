using InventoryManagementSystem.DataAccess.Models;

namespace InventoryManagementSystem.Presentation.Models
{
    public class DashboardViewModel
    {
        public int TotalProducts { get; set; }

        public int TotalCategories { get; set; }

        public List<Product> LowStockProducts { get; set; } = new();

        public List<string> CategoryNames { get; set; } = new();

        public List<int> ProductsPerCategory { get; set; } = new();

        public List<string> TopProductNames { get; set; } = new();

        public List<int> TopProductQuantities { get; set; } = new();
    }
}