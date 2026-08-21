using System.ComponentModel.DataAnnotations;

namespace InventoryManagementSystem.Business.DTOs
{
    public class ProductFormDto
    {
        public int Id { get; set; }



        [Required(ErrorMessage = "Product name is required.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters.")]
        [Display(Name = "Product name")]
        public string Name { get; set; } = string.Empty;



        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        [Display(Name = "Description")]
        public string? Description { get; set; }



        [Range(0.01, 1000000, ErrorMessage = "Price must be greater than zero.")]
        [Display(Name = "Price")]
        public decimal Price { get; set; }



        [Range(0, int.MaxValue, ErrorMessage = "Stock quantity cannot be negative.")]
        [Display(Name = "Stock quantity")]
        public int StockQuantity { get; set; }



        [Range(0, int.MaxValue, ErrorMessage = "Minimum stock level cannot be negative.")]
        [Display(Name = "Minimum stock level")]
        public int MinimumStockLevel { get; set; }



        [Range(1, int.MaxValue, ErrorMessage = "Please select a category.")]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }
    }
}