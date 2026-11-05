namespace EnterpriseConnect.IdentityService.Application.Common.Interfaces;

/// <summary>
/// Cung cấp URL công khai (đi qua API Gateway) để sinh các liên kết gửi ra ngoài (email, thông báo...).
/// </summary>
public interface IPublicUrlProvider
{
    /// <summary>
    /// Base URL công khai của IdentityService khi được truy cập qua API Gateway.
    /// Ví dụ: https://localhost:7272/identity
    /// </summary>
    string IdentityBaseUrl { get; }
}
