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

        // 1. Đọc cấu hình từ IConfiguration và biến môi trường
        var cloudName = configuration["CloudinarySettings:CloudName"]
            ?? Environment.GetEnvironmentVariable("CloudinarySettings__CloudName")
            ?? Environment.GetEnvironmentVariable("CLOUDINARY_CLOUD_NAME");

        var apiKey = configuration["CloudinarySettings:ApiKey"]
            ?? Environment.GetEnvironmentVariable("CloudinarySettings__ApiKey")
            ?? Environment.GetEnvironmentVariable("CLOUDINARY_API_KEY");

        var apiSecret = configuration["CloudinarySettings:ApiSecret"]
            ?? Environment.GetEnvironmentVariable("CloudinarySettings__ApiSecret")
            ?? Environment.GetEnvironmentVariable("CLOUDINARY_API_SECRET");

        // 2. Fail-Fast: Tuyệt đối không hardcode credentials trong mã nguồn
        if (string.IsNullOrWhiteSpace(cloudName) || cloudName.Equals("demo", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Cấu hình bắt buộc 'CloudinarySettings:CloudName' không được để trống hoặc dùng giá trị mặc định 'demo'. Vui lòng cấu hình biến môi trường.");
        }
        if (string.IsNullOrWhiteSpace(apiKey) || apiKey == "123456789")
        {
            throw new InvalidOperationException("Cấu hình bắt buộc 'CloudinarySettings:ApiKey' không được để trống. Vui lòng cấu hình biến môi trường.");
        }
        if (string.IsNullOrWhiteSpace(apiSecret) || apiSecret == "dummy_secret")
        {
            throw new InvalidOperationException("Cấu hình bắt buộc 'CloudinarySettings:ApiSecret' không được để trống. Vui lòng cấu hình biến môi trường.");
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
