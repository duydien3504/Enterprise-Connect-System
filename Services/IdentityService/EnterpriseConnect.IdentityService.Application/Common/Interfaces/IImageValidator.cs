namespace EnterpriseConnect.IdentityService.Application.Common.Interfaces;

public interface IImageValidator
{
    Task<byte[]> ValidateAndSanitizeAvatarAsync(Stream fileStream, string originalFileName, CancellationToken cancellationToken = default);
}
