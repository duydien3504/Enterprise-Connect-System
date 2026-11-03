using EnterpriseConnect.IdentityService.Application.Common.Interfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace EnterpriseConnect.IdentityService.Infrastructure.Services;

public class ImageSharpValidator : IImageValidator
{
    private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB
    private const int MaxDimensionPixels = 4096;
    private const long MaxTotalPixels = 4096 * 4096; // 16.7 MP

    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp"
    };

    public async Task<byte[]> ValidateAndSanitizeAvatarAsync(Stream fileStream, string originalFileName, CancellationToken cancellationToken = default)
    {
        if (fileStream == null || fileStream.Length == 0)
        {
            throw new ArgumentException("Tệp tin ảnh không được để trống.");
        }

        if (fileStream.Length > MaxFileSizeBytes)
        {
            throw new ArgumentException("Dung lượng tệp tin ảnh vượt quá giới hạn cho phép (tối đa 5 MB).");
        }

        var ext = Path.GetExtension(originalFileName);
        if (string.IsNullOrEmpty(ext) || !AllowedExtensions.Contains(ext))
        {
            throw new ArgumentException("Định dạng tệp không được hỗ trợ. Chỉ chấp nhận các định dạng JPG, JPEG, PNG, WEBP.");
        }

        // 1. Kiểm tra Magic Bytes
        fileStream.Position = 0;
        var headerBytes = new byte[8];
        var bytesRead = await fileStream.ReadAsync(headerBytes, 0, headerBytes.Length, cancellationToken);
        if (bytesRead < 4 || !IsValidImageMagicBytes(headerBytes))
        {
            throw new ArgumentException("Dữ liệu tệp không phải là định dạng hình ảnh hợp lệ (Magic Bytes không khớp).");
        }

        // 2. Kiểm tra Dimensions & Total Pixels (Chống Decompression Bomb / Pixel Flood)
        fileStream.Position = 0;
        ImageInfo? imageInfo;
        try
        {
            imageInfo = await Image.IdentifyAsync(fileStream, cancellationToken);
            if (imageInfo == null)
            {
                throw new ArgumentException("Không thể đọc thông tin hình ảnh.");
            }
        }
        catch (Exception ex)
        {
            throw new ArgumentException($"Tệp hình ảnh bị hỏng hoặc không đúng định dạng: {ex.Message}");
        }

        if (imageInfo.Width > MaxDimensionPixels || imageInfo.Height > MaxDimensionPixels)
        {
            throw new ArgumentException($"Kích thước ảnh vượt quá giới hạn tối đa cho phép ({MaxDimensionPixels}x{MaxDimensionPixels} pixels).");
        }

        long totalPixels = (long)imageInfo.Width * imageInfo.Height;
        if (totalPixels > MaxTotalPixels)
        {
            throw new ArgumentException("Tổng số điểm ảnh vượt quá ngưỡng an toàn cho phép.");
        }

        // 3. Giải mã và Re-encode ảnh sạch (Sanitize EXIF scripts / metadata)
        fileStream.Position = 0;
        using var image = await Image.LoadAsync(fileStream, cancellationToken);

        // Resize avatar về tối đa 500x500
        image.Mutate(x => x.Resize(new ResizeOptions
        {
            Size = new Size(500, 500),
            Mode = ResizeMode.Crop
        }));

        using var memoryStream = new MemoryStream();
        await image.SaveAsJpegAsync(memoryStream, new JpegEncoder { Quality = 85 }, cancellationToken);
        return memoryStream.ToArray();
    }

    private static bool IsValidImageMagicBytes(byte[] header)
    {
        // JPEG: FF D8 FF
        if (header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF)
            return true;

        // PNG: 89 50 4E 47 0D 0A 1A 0A
        if (header.Length >= 8 &&
            header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47 &&
            header[4] == 0x0D && header[5] == 0x0A && header[6] == 0x1A && header[7] == 0x0A)
            return true;

        // WEBP: RIFF .... WEBP
        if (header.Length >= 4 &&
            header[0] == 0x52 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x46)
            return true;

        return false;
    }
}
