using System.ComponentModel.DataAnnotations;
using InventoryManagementSystem.DataAccess.Models;

namespace InventoryManagementSystem.Business.DTOs
{
    public class StockFormDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "Please select a product.")]
        [Display(Name = "Product")]
        public int ProductId { get; set; }



        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        [Display(Name = "Quantity")]
        public int Quantity { get; set; } = 1;



        [Required(ErrorMessage = "Transaction type is required.")]
        [Display(Name = "Transaction type")]
        public string TransactionType { get; set; } = TransactionTypes.In;
    }
}