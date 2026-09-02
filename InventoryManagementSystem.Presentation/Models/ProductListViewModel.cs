using InventoryManagementSystem.Business.DTOs;
using InventoryManagementSystem.DataAccess.Models;

namespace InventoryManagementSystem.Presentation.Models
{
    public class ProductListViewModel
    {
        public PagedResult<Product> Page { get; set; } = new();

        public string? Search { get; set; }

        public int? CategoryId { get; set; }
    }
}
