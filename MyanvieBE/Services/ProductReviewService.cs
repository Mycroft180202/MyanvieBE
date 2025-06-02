// MyanvieBE/Services/ProductReviewService.cs
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyanvieBE.Data;
using MyanvieBE.DTOs.Review;
using MyanvieBE.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyanvieBE.Services
{
    public class ProductReviewService : IProductReviewService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<ProductReviewService> _logger;

        public ProductReviewService(ApplicationDbContext context, IMapper mapper, ILogger<ProductReviewService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<ProductReviewDto>> GetReviewsForProductAsync(Guid productId)
        {
            _logger.LogInformation("Fetching reviews for Product ID: {ProductId}", productId);
            var reviews = await _context.ProductReviews
                .Where(r => r.ProductId == productId)
                .Include(r => r.User) // Để lấy UserFullName
                .OrderByDescending(r => r.CreatedAt)
                .AsNoTracking()
                .ToListAsync();
            return _mapper.Map<IEnumerable<ProductReviewDto>>(reviews);
        }

        public async Task<ProductReviewDto?> AddReviewAsync(Guid productId, CreateProductReviewDto reviewDto, Guid userId)
        {
            _logger.LogInformation("Attempting to add review for Product ID: {ProductId} by User ID: {UserId}", productId, userId);

            // 1. Kiểm tra sản phẩm có tồn tại không
            var productExists = await _context.Products.AnyAsync(p => p.Id == productId);
            if (!productExists)
            {
                _logger.LogWarning("Product with ID {ProductId} not found. Cannot add review.", productId);
                return null; // Hoặc throw KeyNotFoundException
            }

            // 2. (QUAN TRỌNG) Kiểm tra xem user này đã đánh giá sản phẩm này chưa
            // Thông thường, một user chỉ được đánh giá một sản phẩm một lần.
            var existingReview = await _context.ProductReviews
                .FirstOrDefaultAsync(r => r.ProductId == productId && r.UserId == userId);

            if (existingReview != null)
            {
                _logger.LogWarning("User {UserId} has already reviewed Product {ProductId}. Cannot add another review.", userId, productId);
                // Hoặc throw một InvalidOperationException với thông báo cụ thể
                // Ví dụ: throw new InvalidOperationException("Bạn đã đánh giá sản phẩm này rồi.");
                return null; // Trả về null để controller xử lý (ví dụ: trả về 409 Conflict)
            }

            // (Tùy chọn: Kiểm tra xem user đã mua sản phẩm này chưa - logic phức tạp hơn, cần xem xét Orders)
            // For now, we allow any logged-in user to review.

            var review = _mapper.Map<ProductReview>(reviewDto);
            review.ProductId = productId;
            review.UserId = userId;
            // CreatedAt, UpdatedAt, Id sẽ được BaseEntity tự xử lý

            await _context.ProductReviews.AddAsync(review);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Review added successfully with ID: {ReviewId} for Product ID: {ProductId} by User ID: {UserId}", review.Id, productId, userId);

            // Load lại User để map UserFullName
            await _context.Entry(review).Reference(r => r.User).LoadAsync();
            return _mapper.Map<ProductReviewDto>(review);
        }
    }
}