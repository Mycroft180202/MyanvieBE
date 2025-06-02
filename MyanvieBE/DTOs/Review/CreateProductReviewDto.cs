// MyanvieBE/DTOs/Review/CreateProductReviewDto.cs
using System.ComponentModel.DataAnnotations;

namespace MyanvieBE.DTOs.Review
{
    public class CreateProductReviewDto
    {
        // ProductId sẽ được lấy từ URL

        [Required(ErrorMessage = "Đánh giá sao là bắt buộc.")]
        [Range(1, 5, ErrorMessage = "Đánh giá sao phải từ 1 đến 5.")]
        public int Rating { get; set; }

        public string? Comment { get; set; }
    }
}