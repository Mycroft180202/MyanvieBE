// MyanvieBE/Models/CartItem.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyanvieBE.Models
{
    public class CartItem : BaseEntity
    {
        [Required]
        public Guid CartId { get; set; } 

        [Required]
        public Guid ProductId { get; set; }

        [Required]
        public int Quantity { get; set; }

        [ForeignKey("CartId")]
        public virtual Cart Cart { get; set; }

        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }
    }
}