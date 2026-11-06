using EnterpriseConnect.AdminService.Application.Common.Interfaces;
using EnterpriseConnect.AdminService.Application.Common.Mappings;
using EnterpriseConnect.AdminService.Application.Common.Models;
using MediatR;

namespace EnterpriseConnect.AdminService.Application.Features.System.Queries.GetSystemSettings;

public class GetSystemSettingsQueryHandler 
    : IRequestHandler<GetSystemSettingsQuery, Result<List<SystemSettingDto>>>
{
    private readonly IAdminUnitOfWork _unitOfWork;

    public GetSystemSettingsQueryHandler(IAdminUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<List<SystemSettingDto>>> Handle(
        GetSystemSettingsQuery request,
        CancellationToken cancellationToken)
    {
        var settings = await _unitOfWork.SystemSettings.GetAllAsync(request.GroupName, cancellationToken);
        var dtos = settings.Select(x => x.ToDto()).ToList();
        return Result<List<SystemSettingDto>>.Success(dtos);
    }
}
