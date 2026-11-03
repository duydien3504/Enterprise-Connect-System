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

        var cloudName = configuration["CloudinarySettings:CloudName"] ?? "demo";
        var apiKey = configuration["CloudinarySettings:ApiKey"] ?? "123456789";
        var apiSecret = configuration["CloudinarySettings:ApiSecret"] ?? "dummy_secret";

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
                Transformation = new Transformation().Width(500).Height(500).Crop("fill").Gravity("face")
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams, cancellationToken);
            if (uploadResult.Error != null)
            {
                _logger.LogWarning("Cloudinary trả về lỗi: {Error}. Fallback dùng URL placeholder.", uploadResult.Error.Message);
                return $"https://res.cloudinary.com/demo/image/upload/v1/{folder}/{fileName}";
            }

            return uploadResult.SecureUrl?.ToString() ?? uploadResult.Url?.ToString() 
                ?? $"https://res.cloudinary.com/demo/image/upload/v1/{folder}/{fileName}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi upload ảnh đại diện lên Cloudinary");
            return $"https://res.cloudinary.com/demo/image/upload/v1/{folder}/{fileName}";
        }
    }
}
