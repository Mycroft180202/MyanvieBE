using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Minio;
using MyanvieBE.Data;
using MyanvieBE.Services;
using NuGet.Protocol.Plugins;
using System.Text;
using VNPAY.NET;
using Net.payOS;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

var minioSettings = builder.Configuration.GetSection("MinioSettings");
var minioEndpoint = minioSettings["Endpoint"];
var minioAccessKey = minioSettings["AccessKey"];
var minioSecretKey = minioSettings["SecretKey"];
var minioUseSsl = bool.Parse(minioSettings["UseSsl"] ?? "false");

var connectionUrl = builder.Configuration.GetConnectionString("DefaultConnection");

string connectionString;
if (string.IsNullOrWhiteSpace(connectionUrl))
{
    throw new InvalidOperationException("Connection string 'DefaultConnection' not found in configuration.");
}
else
{
    var uri = new Uri(connectionUrl);
    var userInfo = uri.UserInfo.Split(':');

    var builderNpgsql = new Npgsql.NpgsqlConnectionStringBuilder
    {
        Host = uri.Host,
        Port = uri.Port,
        Username = userInfo[0],
        Password = userInfo[1],
        Database = uri.AbsolutePath.Trim('/'),
        // Thêm 2 dòng sau để kết nối an toàn trên môi trường cloud
        SslMode = Npgsql.SslMode.Prefer,
        TrustServerCertificate = true
    };

    connectionString = builderNpgsql.ConnectionString;
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddSingleton<IMinioClient>(sp => new MinioClient()
    .WithEndpoint(minioEndpoint)
    .WithCredentials(minioAccessKey, minioSecretKey)
    .WithSSL(minioUseSsl)
    .Build());

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.SaveToken = true; // Lưu token trong HttpContext sau khi xác thực thành công
    options.RequireHttpsMetadata = false; // Trong development có thể đặt false, production nên là true
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true, // Kiểm tra token có hết hạn không
        ValidateIssuerSigningKey = true, // Quan trọng: Phải xác thực chữ ký

        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        ValidAudience = builder.Configuration["JwtSettings:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Key"]))
    };
});

builder.Services.AddAuthorization();

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ISubCategoryService, SubCategoryService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IMinioService, MinioService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IProductReviewService, ProductReviewService>();
builder.Services.AddScoped<INewsArticleService, NewsArticleService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddSingleton<IVnpay, Vnpay>();
builder.Services.AddSingleton(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var clientId = configuration["PayOS:ClientId"] ?? throw new ArgumentNullException("PayOS:ClientId");
    var apiKey = configuration["PayOS:ApiKey"] ?? throw new ArgumentNullException("PayOS:ApiKey");
    var checksumKey = configuration["PayOS:ChecksumKey"] ?? throw new ArgumentNullException("PayOS:ChecksumKey");
    return new PayOS(clientId, apiKey, checksumKey);
});

builder.Services.AddHttpContextAccessor();

// Thêm cấu hình CORS
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy =>
                      {
                          policy.WithOrigins("https://myanvie.netlify.app")
                                .AllowAnyHeader()
                                .AllowAnyMethod();
                      });
});



builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.Migrate();
}

// Tự động tạo bucket mặc định khi ứng dụng khởi động
using (var scope = app.Services.CreateScope())
{
    var minioService = scope.ServiceProvider.GetRequiredService<IMinioService>();
    var defaultBucketName = builder.Configuration["MinioSettings:BucketName"];
    await minioService.EnsureBucketExistsAsync(defaultBucketName);
}
    // Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(MyAllowSpecificOrigins);
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();