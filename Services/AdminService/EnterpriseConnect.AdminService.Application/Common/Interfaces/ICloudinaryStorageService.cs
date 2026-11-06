namespace EnterpriseConnect.AdminService.Application.Common.Interfaces;

public interface ICloudinaryStorageService
{
    Task<string> UploadRawFileAsync(Stream fileStream, string fileName, string folder = "documents", CancellationToken cancellationToken = default);
}
