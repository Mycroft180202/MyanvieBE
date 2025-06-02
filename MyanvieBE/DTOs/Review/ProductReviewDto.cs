// MyanvieBE/DTOs/Review/ProductReviewDto.cs
using System;

namespace MyanvieBE.DTOs.Review
{
    public class ProductReviewDto
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public Guid UserId { get; set; }
        public string UserFullName { get; set; } // Tên người đánh giá
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}