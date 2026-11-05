namespace EnterpriseConnect.IdentityService.Application.Common.Interfaces;

public interface ICloudinaryStorageService
{
    Task<string> UploadImageAsync(Stream imageStream, string fileName, string folder = "avatars", CancellationToken cancellationToken = default);
}
