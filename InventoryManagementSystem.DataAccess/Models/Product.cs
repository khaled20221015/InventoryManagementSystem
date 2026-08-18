using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventoryManagementSystem.DataAccess.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int StockQuantity { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int MinimumStockLevel { get; set; }

        // FK
        [Required]
        public int CategoryId { get; set; }

        // Navigation Property
        [ForeignKey(nameof(CategoryId))]
        public Category Category { get; set; } = null!; // null! = Null-forgiving operator

        // Navigation Property
        public List<StockTransaction> StockTransactions { get; set; } = new List<StockTransaction>();
    }
}