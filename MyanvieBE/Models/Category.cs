// MyanvieBE/Models/Category.cs
using System.ComponentModel.DataAnnotations;

namespace MyanvieBE.Models
{
    public class Category : BaseEntity
    {
        [Required, MaxLength(100)]
        public string Name { get; set; }
        public string? Description { get; set; }

        public virtual ICollection<SubCategory> SubCategories { get; set; } = new List<SubCategory>();

    }
}