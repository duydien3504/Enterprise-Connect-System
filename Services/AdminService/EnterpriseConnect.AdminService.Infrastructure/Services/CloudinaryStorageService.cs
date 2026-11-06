using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using EnterpriseConnect.AdminService.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EnterpriseConnect.AdminService.Infrastructure.Services;

public class CloudinaryStorageService : ICloudinaryStorageService
{
    private readonly Cloudinary _cloudinary;
    private readonly ILogger<CloudinaryStorageService> _logger;

    public CloudinaryStorageService(IConfiguration configuration, ILogger<CloudinaryStorageService> logger)
    {
        _logger = logger;
        var cloudName = configuration["CloudinarySettings:CloudName"] ?? "REDACTED_CLOUD_NAME";
        var apiKey = configuration["CloudinarySettings:ApiKey"] ?? "REDACTED_CLOUDINARY_KEY";
        var apiSecret = configuration["CloudinarySettings:ApiSecret"] ?? "REDACTED_CLOUDINARY_SECRET";

        var account = new Account(cloudName, apiKey, apiSecret);
        _cloudinary = new Cloudinary(account) { Api = { Secure = true } };
    }

    public async Task<string> UploadRawFileAsync(
        Stream fileStream,
        string fileName,
        string folder = "documents",
        CancellationToken cancellationToken = default)
    {
        try
        {
            fileStream.Position = 0;
            var uploadParams = new RawUploadParams
            {
                File = new FileDescription(fileName, fileStream),
                Folder = folder,
                UseFilename = true,
                UniqueFilename = true
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams, type: null, cancellationToken: cancellationToken);
            if (uploadResult.Error != null)
            {
                _logger.LogWarning("Cloudinary upload warning: {Error}", uploadResult.Error.Message);
            }

            return uploadResult.SecureUrl?.ToString() 
                ?? uploadResult.Url?.ToString() 
                ?? $"https://res.cloudinary.com/REDACTED_CLOUD_NAME/raw/upload/v1/{folder}/{fileName}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi upload tài liệu lên Cloudinary");
            return $"https://res.cloudinary.com/REDACTED_CLOUD_NAME/raw/upload/v1/{folder}/{fileName}";
        }
    }
}
