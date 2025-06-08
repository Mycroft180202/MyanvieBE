// MyanvieBE/DTOs/Review/ProductReviewDto.cs
namespace MyanvieBE.DTOs.Review
{
    public class ProductReviewDto
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public Guid UserId { get; set; }
        public string UserFullName { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}