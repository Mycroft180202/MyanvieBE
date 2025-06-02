// MyanvieBE/Services/IProductReviewService.cs
using MyanvieBE.DTOs.Review;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyanvieBE.Services
{
    public interface IProductReviewService
    {
        Task<IEnumerable<ProductReviewDto>> GetReviewsForProductAsync(Guid productId);
        Task<ProductReviewDto?> AddReviewAsync(Guid productId, CreateProductReviewDto reviewDto, Guid userId);
    }
}