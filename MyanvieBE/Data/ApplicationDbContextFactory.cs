// MyanvieBE/Data/ApplicationDbContextFactory.cs (hoặc MyanvieBE/ApplicationDbContextFactory.cs)
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;
using MyanvieBE.Data; // Đảm bảo using đúng namespace tới ApplicationDbContext

public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        string basePath = Directory.GetCurrentDirectory();
        if (basePath.EndsWith("Data")) // Hoặc kiểm tra cụ thể hơn
        {
            basePath = Path.GetFullPath(Path.Combine(basePath, ".."));
        }


        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(basePath) // Đặt thư mục gốc để tìm file cấu hình
            .AddJsonFile("appsettings.Development.json", optional: true) // Ưu tiên Development
            .AddJsonFile("appsettings.json", optional: true) // Sau đó là appsettings.json chung
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        }

        optionsBuilder.UseNpgsql(connectionString);

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}