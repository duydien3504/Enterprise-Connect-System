using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using EnterpriseConnect.IdentityService.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EnterpriseConnect.IdentityService.Infrastructure.Services;

public class CloudinaryStorageService : ICloudinaryStorageService
{
    private readonly Cloudinary _cloudinary;
    private readonly ILogger<CloudinaryStorageService> _logger;

    public CloudinaryStorageService(IConfiguration configuration, ILogger<CloudinaryStorageService> logger)
    {
        _logger = logger;

        // 1. Ưu tiên đọc từ IConfiguration, sau đó kiểm tra biến môi trường hệ thống
        var cloudName = configuration["CloudinarySettings:CloudName"]
            ?? Environment.GetEnvironmentVariable("CloudinarySettings__CloudName")
            ?? Environment.GetEnvironmentVariable("CLOUDINARY_CLOUD_NAME");

        var apiKey = configuration["CloudinarySettings:ApiKey"]
            ?? Environment.GetEnvironmentVariable("CloudinarySettings__ApiKey")
            ?? Environment.GetEnvironmentVariable("CLOUDINARY_API_KEY");

        var apiSecret = configuration["CloudinarySettings:ApiSecret"]
            ?? Environment.GetEnvironmentVariable("CloudinarySettings__ApiSecret")
            ?? Environment.GetEnvironmentVariable("CLOUDINARY_API_SECRET");

        // 2. Fallback an toàn: Nếu môi trường chưa cấu hình biến môi trường hoặc mang giá trị placeholder "demo"
        if (string.IsNullOrWhiteSpace(cloudName) || cloudName.Equals("demo", StringComparison.OrdinalIgnoreCase))
        {
            cloudName = "REDACTED_CLOUD_NAME";
        }
        if (string.IsNullOrWhiteSpace(apiKey) || apiKey == "123456789")
        {
            apiKey = "REDACTED_CLOUDINARY_KEY";
        }
        if (string.IsNullOrWhiteSpace(apiSecret) || apiSecret == "dummy_secret")
        {
            apiSecret = "REDACTED_CLOUDINARY_SECRET";
        }

        var account = new Account(cloudName, apiKey, apiSecret);
        _cloudinary = new Cloudinary(account);
        _cloudinary.Api.Secure = true;
    }

    public async Task<string> UploadImageAsync(Stream imageStream, string fileName, string folder = "avatars", CancellationToken cancellationToken = default)
    {
        try
        {
            imageStream.Position = 0;
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(fileName, imageStream),
                Folder = folder,
                PublicId = Path.GetFileNameWithoutExtension(fileName),
                Overwrite = true,
                Transformation = new Transformation().Width(500).Height(500).Crop("fill").Gravity("face")
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams, cancellationToken);
            if (uploadResult.Error != null)
            {
                _logger.LogError("Cloudinary upload error: {Error} (StatusCode: {StatusCode})", 
                    uploadResult.Error.Message, uploadResult.StatusCode);
                throw new InvalidOperationException($"Lưu trữ ảnh Cloudinary thất bại: {uploadResult.Error.Message}");
            }

            var resultUrl = uploadResult.SecureUrl?.ToString() ?? uploadResult.Url?.ToString();
            if (string.IsNullOrWhiteSpace(resultUrl))
            {
                throw new InvalidOperationException("Cloudinary không trả về URL hình ảnh hợp lệ.");
            }

            _logger.LogInformation("Upload ảnh đại diện thành công lên Cloudinary: {Url}", resultUrl);
            return resultUrl;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi ngoại lệ khi upload ảnh đại diện lên Cloudinary");
            throw;
        }
    }
}
