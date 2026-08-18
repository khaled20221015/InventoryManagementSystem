using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using InventoryManagementSystem.DataAccess.Identity;

namespace InventoryManagementSystem.DataAccess.Models
{
    public class StockTransaction
    {
        [Key]
        public int Id { get; set; }


        // FK → Product
        [Required]
        public int ProductId { get; set; }

        // FK → ApplicationUser
        [Required]
        public string UserId { get; set; } = string.Empty;



        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }


        [Required]
        [MaxLength(20)]
        public string TransactionType { get; set; } = string.Empty;


        [Required]
        public DateTime TransactionDate { get; set; }




        // Navigation Property → Product
        [ForeignKey(nameof(ProductId))]
        public Product Product { get; set; } = null!;


        // Navigation Property → ApplicationUser
        [ForeignKey(nameof(UserId))]
        public ApplicationUser User { get; set; } = null!;
    }
}