using EnterpriseConnect.AdminService.Application.Common.Models;
using MediatR;

namespace EnterpriseConnect.AdminService.Application.Features.System.Queries.GetSystemSettings;

public class GetSystemSettingsQuery : IRequest<Result<List<SystemSettingDto>>>
{
    public string? GroupName { get; set; }
}
