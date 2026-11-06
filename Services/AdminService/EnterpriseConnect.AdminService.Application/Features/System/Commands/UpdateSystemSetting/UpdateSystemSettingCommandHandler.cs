using EnterpriseConnect.AdminService.Application.Common.Interfaces;
using EnterpriseConnect.AdminService.Application.Common.Mappings;
using EnterpriseConnect.AdminService.Application.Common.Models;
using EnterpriseConnect.AdminService.Domain.Entities;
using MediatR;

namespace EnterpriseConnect.AdminService.Application.Features.System.Commands.UpdateSystemSetting;

public class UpdateSystemSettingCommandHandler 
    : IRequestHandler<UpdateSystemSettingCommand, Result<SystemSettingDto>>
{
    private readonly IAdminUnitOfWork _unitOfWork;

    public UpdateSystemSettingCommandHandler(IAdminUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<SystemSettingDto>> Handle(
        UpdateSystemSettingCommand request,
        CancellationToken cancellationToken)
    {
        var setting = await _unitOfWork.SystemSettings.GetByKeyAsync(request.SettingKey, cancellationToken);
        if (setting == null)
        {
            setting = new SystemSetting
            {
                SettingKey = request.SettingKey,
                SettingValue = request.SettingValue,
                GroupName = request.GroupName,
                Description = request.Description,
                IsPublic = request.IsPublic
            };
        }
        else
        {
            setting.SettingValue = request.SettingValue;
            setting.GroupName = request.GroupName;
            setting.Description = request.Description;
            setting.IsPublic = request.IsPublic;
            setting.UpdatedAt = DateTimeOffset.UtcNow;
        }

        await _unitOfWork.SystemSettings.AddOrUpdateAsync(setting, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<SystemSettingDto>.Success(setting.ToDto());
    }
}
