// MyanvieBE/Mappings/MappingProfile.cs
using AutoMapper;
using MyanvieBE.DTOs.Auth;
using MyanvieBE.DTOs.Category; // Giữ lại nếu bạn có Category DTOs
using MyanvieBE.DTOs.News;
using MyanvieBE.DTOs.Order;
using MyanvieBE.DTOs.Product;
using MyanvieBE.DTOs.Review;
using MyanvieBE.Models;
using System.Linq;

namespace MyanvieBE.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Product, ProductDto>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name));

            CreateMap<CreateProductDto, Product>(); // Map này giờ sẽ map cả Color, Size, Price, Stock, Sku

            // Category Mappings (Giữ nguyên)
            CreateMap<Category, CategoryDto>();
            CreateMap<CreateCategoryDto, Category>();
            CreateMap<User, UserDto>();
            CreateMap<AdminUpdateUserDto, User>()
                .ForMember(dest => dest.Email, opt => opt.Ignore()) // Không cho phép cập nhật Email qua DTO này
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore()) // Không cập nhật mật khẩu qua DTO này
                .ForMember(dest => dest.PasswordSalt, opt => opt.Ignore()); // Không cập nhật mật khẩu qua DTO này
            CreateMap<CreateOrderItemDto, OrderItem>()
                .ForMember(dest => dest.Price, opt => opt.Ignore()); // Giá sẽ được set trong service

            // Map từ Order (model) sang OrderDto (để trả về)
            CreateMap<Order, OrderDto>()
                .ForMember(dest => dest.CustomerFullName, opt => opt.MapFrom(src => src.User.FullName))
                .ForMember(dest => dest.CustomerEmail, opt => opt.MapFrom(src => src.User.Email));
            // TotalAmount sẽ được tính và gán trong service, hoặc nếu Order model có sẵn thì map trực tiếp

            // Map từ OrderItem (model) sang OrderItemDto (để trả về)
            CreateMap<OrderItem, OrderItemDto>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name))
                .ForMember(dest => dest.ProductThumbnailUrl, opt => opt.MapFrom(src => src.Product.ThumbnailUrl));

            CreateMap<ProductReview, ProductReviewDto>()
                .ForMember(dest => dest.UserFullName, opt => opt.MapFrom(src => src.User.FullName));

            CreateMap<CreateProductReviewDto, ProductReview>();

            CreateMap<NewsArticle, NewsArticleDto>()
                .ForMember(dest => dest.AuthorFullName, opt => opt.MapFrom(src => src.Author.FullName));

            CreateMap<CreateNewsArticleDto, NewsArticle>()
                .ForMember(dest => dest.AuthorId, opt => opt.Ignore()) // Sẽ gán AuthorId trong service
                .ForMember(dest => dest.Author, opt => opt.Ignore());
        }
    }
}