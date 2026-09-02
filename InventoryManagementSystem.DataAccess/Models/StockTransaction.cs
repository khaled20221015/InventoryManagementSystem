using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using InventoryManagementSystem.DataAccess.Identity;

namespace InventoryManagementSystem.DataAccess.Models
{
    // One stock movement: who moved how much of which product, in or out.
    public class StockTransaction
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        // Either TransactionTypes.In or TransactionTypes.Out.
        [Required]
        [MaxLength(20)]
        public string TransactionType { get; set; } = string.Empty;

        [Required]
        public DateTime TransactionDate { get; set; }

        [ForeignKey(nameof(ProductId))]
        public Product Product { get; set; } = null!;

        [ForeignKey(nameof(UserId))]
        public ApplicationUser User { get; set; } = null!;
    }
}
