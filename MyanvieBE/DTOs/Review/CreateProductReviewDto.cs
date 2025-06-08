// MyanvieBE/DTOs/Review/CreateProductReviewDto.cs
using System.ComponentModel.DataAnnotations;

namespace MyanvieBE.DTOs.Review
{
    public class CreateProductReviewDto
    {
        [Required]
        public Guid ProductId { get; set; }

        [Required, Range(1, 5)]
        public int Rating { get; set; }

        public string? Comment { get; set; }
    }
}