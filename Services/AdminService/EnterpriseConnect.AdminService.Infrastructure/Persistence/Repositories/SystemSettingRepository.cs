using EnterpriseConnect.AdminService.Application.Common.Interfaces;
using EnterpriseConnect.AdminService.Domain.Entities;
using EnterpriseConnect.AdminService.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseConnect.AdminService.Infrastructure.Persistence.Repositories;

public class SystemSettingRepository : ISystemSettingRepository
{
    private readonly AdminDbContext _context;

    public SystemSettingRepository(AdminDbContext context)
    {
        _context = context;
    }

    public async Task<SystemSetting?> GetByKeyAsync(string key, CancellationToken cancellationToken = default)
    {
        return await _context.SystemSettings.FirstOrDefaultAsync(x => x.SettingKey == key, cancellationToken);
    }

    public async Task<List<SystemSetting>> GetAllAsync(string? groupName, CancellationToken cancellationToken = default)
    {
        var query = _context.SystemSettings.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(groupName))
            query = query.Where(x => x.GroupName == groupName);

        return await query.OrderBy(x => x.SettingKey).ToListAsync(cancellationToken);
    }

    public async Task AddOrUpdateAsync(SystemSetting setting, CancellationToken cancellationToken = default)
    {
        var existing = await _context.SystemSettings.FirstOrDefaultAsync(x => x.SettingKey == setting.SettingKey, cancellationToken);
        if (existing == null)
        {
            await _context.SystemSettings.AddAsync(setting, cancellationToken);
        }
        else
        {
            existing.SettingValue = setting.SettingValue;
            existing.GroupName = setting.GroupName;
            existing.Description = setting.Description;
            existing.IsPublic = setting.IsPublic;
            existing.UpdatedAt = DateTimeOffset.UtcNow;
            _context.SystemSettings.Update(existing);
        }
    }
}
