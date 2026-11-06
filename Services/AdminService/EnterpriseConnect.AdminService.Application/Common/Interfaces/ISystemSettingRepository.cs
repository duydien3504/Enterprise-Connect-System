using EnterpriseConnect.AdminService.Domain.Entities;

namespace EnterpriseConnect.AdminService.Application.Common.Interfaces;

public interface ISystemSettingRepository
{
    Task<SystemSetting?> GetByKeyAsync(string key, CancellationToken cancellationToken = default);
    Task<List<SystemSetting>> GetAllAsync(string? groupName, CancellationToken cancellationToken = default);
    Task AddOrUpdateAsync(SystemSetting setting, CancellationToken cancellationToken = default);
}
