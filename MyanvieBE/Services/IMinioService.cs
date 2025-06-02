// MyanvieBE/Services/IMinioService.cs
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace MyanvieBE.Services
{
    public interface IMinioService
    {
        Task<string> UploadFileAsync(IFormFile file, string? bucketName = null);
        Task EnsureBucketExistsAsync(string? bucketName = null);
    }
}