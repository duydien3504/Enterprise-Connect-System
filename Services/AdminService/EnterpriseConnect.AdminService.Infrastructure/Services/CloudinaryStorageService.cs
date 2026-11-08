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
        var cloudName = configuration["CloudinarySettings:CloudName"]
            ?? Environment.GetEnvironmentVariable("CloudinarySettings__CloudName")
            ?? Environment.GetEnvironmentVariable("CLOUDINARY_CLOUD_NAME")
            ?? string.Empty;
        var apiKey = configuration["CloudinarySettings:ApiKey"]
            ?? Environment.GetEnvironmentVariable("CloudinarySettings__ApiKey")
            ?? Environment.GetEnvironmentVariable("CLOUDINARY_API_KEY")
            ?? string.Empty;
        var apiSecret = configuration["CloudinarySettings:ApiSecret"]
            ?? Environment.GetEnvironmentVariable("CloudinarySettings__ApiSecret")
            ?? Environment.GetEnvironmentVariable("CLOUDINARY_API_SECRET")
            ?? string.Empty;

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
                _logger.LogError("Cloudinary upload error: {Error}", uploadResult.Error.Message);
                throw new InvalidOperationException($"Lưu trữ tài liệu Cloudinary thất bại: {uploadResult.Error.Message}");
            }

            var resultUrl = uploadResult.SecureUrl?.ToString() ?? uploadResult.Url?.ToString();
            if (string.IsNullOrWhiteSpace(resultUrl))
            {
                throw new InvalidOperationException("Cloudinary không trả về URL tài liệu hợp lệ.");
            }

            return resultUrl;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi upload tài liệu lên Cloudinary");
            throw;
        }
    }
}
