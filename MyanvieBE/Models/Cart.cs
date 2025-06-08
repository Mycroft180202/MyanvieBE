using System.ComponentModel.DataAnnotations.Schema;

namespace MyanvieBE.Models
{
    public class Cart : BaseEntity
    {
        [ForeignKey("User")]
        public Guid UserId { get; set; }
        public virtual User User { get; set; }

        public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    }
}