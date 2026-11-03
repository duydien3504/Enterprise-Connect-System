namespace EnterpriseConnect.IdentityService.Application.Common.Interfaces;

public interface ICacheService
{
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default);
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
}

public interface IEmailSender
{
    Task SendEmailAsync(string toEmail, string subject, string bodyHtml, CancellationToken cancellationToken = default);
}

public interface ICloudinaryStorageService
{
    Task<string> UploadImageAsync(Stream imageStream, string fileName, string folder = "avatars", CancellationToken cancellationToken = default);
}

public interface IImageValidator
{
    Task<byte[]> ValidateAndSanitizeAvatarAsync(Stream fileStream, string originalFileName, CancellationToken cancellationToken = default);
}
