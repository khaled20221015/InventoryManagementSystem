using System.ComponentModel.DataAnnotations;

namespace InventoryManagementSystem.DataAccess.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        public List<Product> Products { get; set; } = new();
    }
}
