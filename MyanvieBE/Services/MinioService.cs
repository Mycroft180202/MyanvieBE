// MyanvieBE/Services/MinioService.cs
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Minio;
using Minio.DataModel.Args;
using Minio.Exceptions;
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace MyanvieBE.Services
{
    public class MinioService : IMinioService
    {
        private readonly IMinioClient _minioClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<MinioService> _logger;
        private readonly string _defaultBucketName;
        private readonly string _publicBaseUrl;

        public MinioService(IMinioClient minioClient, IConfiguration configuration, ILogger<MinioService> logger)
        {
            _minioClient = minioClient;
            _configuration = configuration;
            _logger = logger;
            _defaultBucketName = _configuration["MinioSettings:BucketName"] ?? "default-bucket";
            _publicBaseUrl = _configuration["MinioSettings:PublicBaseUrl"] ?? $"http://{_configuration["MinioSettings:Endpoint"]}";
        }

        public async Task EnsureBucketExistsAsync(string? bucketName = null)
        {
            var targetBucketName = bucketName ?? _defaultBucketName;
            try
            {
                _logger.LogInformation("Checking if bucket '{BucketName}' exists...", targetBucketName);
                bool found = await _minioClient.BucketExistsAsync(new BucketExistsArgs().WithBucket(targetBucketName));
                if (!found)
                {
                    _logger.LogInformation("Bucket '{BucketName}' not found. Creating new bucket...", targetBucketName);
                    await _minioClient.MakeBucketAsync(new MakeBucketArgs().WithBucket(targetBucketName));
                    _logger.LogInformation("Bucket '{BucketName}' created successfully.", targetBucketName);

                    // Cấu hình policy để public read cho bucket (tùy chọn, cẩn thận với dữ liệu nhạy cảm)
                    // Đối với ảnh sản phẩm, tin tức thì thường là public read
                    string policyJson = $@"{{
                        ""Version"": ""2012-10-17"",
                        ""Statement"": [
                            {{
                                ""Effect"": ""Allow"",
                                ""Principal"": {{""AWS"":[""*""]}},
                                ""Action"": [""s3:GetObject""],
                                ""Resource"": [""arn:aws:s3:::{targetBucketName}/*""]
                            }}
                        ]
                    }}";
                    await _minioClient.SetPolicyAsync(new SetPolicyArgs().WithBucket(targetBucketName).WithPolicy(policyJson));
                    _logger.LogInformation("Public read policy set for bucket '{BucketName}'.", targetBucketName);

                }
                else
                {
                    _logger.LogInformation("Bucket '{BucketName}' already exists.", targetBucketName);
                }
            }
            catch (MinioException e)
            {
                _logger.LogError(e, "MinIO Error while ensuring bucket exists: {BucketName}", targetBucketName);
                throw;
            }
        }

        public async Task<string> UploadFileAsync(IFormFile file, string? bucketName = null)
        {
            var targetBucketName = bucketName ?? _defaultBucketName;
            await EnsureBucketExistsAsync(targetBucketName); // Đảm bảo bucket tồn tại

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);

            try
            {
                _logger.LogInformation("Attempting to upload file {FileName} to bucket {BucketName}", fileName, targetBucketName);

                using (var stream = file.OpenReadStream())
                {
                    var putObjectArgs = new PutObjectArgs()
                        .WithBucket(targetBucketName)
                        .WithObject(fileName)
                        .WithStreamData(stream)
                        .WithObjectSize(file.Length)
                        .WithContentType(file.ContentType);
                    await _minioClient.PutObjectAsync(putObjectArgs);
                }

                _logger.LogInformation("File {FileName} uploaded successfully to bucket {BucketName}", fileName, targetBucketName);

                // Trả về URL công khai để truy cập file
                return $"{_publicBaseUrl.TrimEnd('/')}/{targetBucketName}/{fileName}";
            }
            catch (MinioException e)
            {
                _logger.LogError(e, "MinIO Error during file upload: {FileName} to bucket {BucketName}", fileName, targetBucketName);
                throw;
            }
        }
    }
}