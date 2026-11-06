using EnterpriseConnect.AdminService.Application.Common.Models;
using EnterpriseConnect.AdminService.Domain.Entities;

namespace EnterpriseConnect.AdminService.Application.Common.Mappings;

public static class SystemSettingMappingExtensions
{
    public static SystemSettingDto ToDto(this SystemSetting entity)
    {
        return new SystemSettingDto
        {
            Id = entity.Id,
            SettingKey = entity.SettingKey,
            SettingValue = entity.SettingValue,
            GroupName = entity.GroupName,
            Description = entity.Description,
            IsPublic = entity.IsPublic
        };
    }
}
