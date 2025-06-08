// MyanvieBE/Mappings/MappingProfile.cs
using AutoMapper;
using MyanvieBE.DTOs;
using MyanvieBE.DTOs.Auth;
using MyanvieBE.DTOs.Cart;
using MyanvieBE.DTOs.Category;
using MyanvieBE.DTOs.News;
using MyanvieBE.DTOs.Order;
using MyanvieBE.DTOs.Product;
using MyanvieBE.DTOs.Review;
using MyanvieBE.DTOs.SubCategory; // Thêm using
using MyanvieBE.Models;
using System.Linq;

namespace MyanvieBE.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // User & Auth Mappings (Giữ nguyên)
            CreateMap<User, UserDto>();
            CreateMap<RegisterDto, User>();
            CreateMap<AdminUpdateUserDto, User>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // Product Mappings (Cập nhật)
            CreateMap<Product, ProductDto>()
                .ForMember(dest => dest.SubCategoryName, opt => opt.MapFrom(src => src.SubCategory.Name))
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.SubCategory.Category.Name));
            CreateMap<CreateProductDto, Product>();

            // Category Mappings (Giữ nguyên)
            CreateMap<Category, CategoryDto>();
            CreateMap<CreateCategoryDto, Category>();

            // SubCategory Mappings (Mới)
            CreateMap<SubCategory, SubCategoryDto>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name));
            CreateMap<CreateSubCategoryDto, SubCategory>();

            // Cart Mappings (Giữ nguyên)
            CreateMap<Cart, CartDto>()
                 .ForMember(dest => dest.TotalPrice, opt => opt.MapFrom(src => src.CartItems.Sum(item => item.Quantity * item.Product.Price)));
            CreateMap<CartItem, CartItemDto>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name))
                .ForMember(dest => dest.ProductPrice, opt => opt.MapFrom(src => src.Product.Price))
                .ForMember(dest => dest.ProductImage, opt => opt.MapFrom(src => src.Product.ThumbnailUrl));
            CreateMap<AddCartItemDto, CartItem>();

            // Order Mappings (Giữ nguyên)
            CreateMap<CreateOrderDto, Order>();
            CreateMap<Order, OrderDto>()
                .ForMember(dest => dest.CustomerFullName, opt => opt.MapFrom(src => src.User.FullName))
                .ForMember(dest => dest.CustomerEmail, opt => opt.MapFrom(src => src.User.Email));
            CreateMap<OrderItem, OrderItemDto>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name))
                .ForMember(dest => dest.ProductThumbnailUrl, opt => opt.MapFrom(src => src.Product.ThumbnailUrl));

            // Review Mappings (Giữ nguyên)
            CreateMap<ProductReview, ProductReviewDto>()
                .ForMember(dest => dest.UserFullName, opt => opt.MapFrom(src => src.User.FullName));
            CreateMap<CreateProductReviewDto, ProductReview>();

            // News Mappings (Giữ nguyên)
            CreateMap<NewsArticle, NewsArticleDto>()
                .ForMember(dest => dest.AuthorFullName, opt => opt.MapFrom(src => src.Author.FullName));
            CreateMap<CreateNewsArticleDto, NewsArticle>()
                .ForMember(dest => dest.AuthorId, opt => opt.Ignore())
                .ForMember(dest => dest.Author, opt => opt.Ignore());
        }
    }
}