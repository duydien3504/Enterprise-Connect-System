using EnterpriseConnect.AdminService.Application.Common.Models;
using EnterpriseConnect.AdminService.Domain.Entities;

namespace EnterpriseConnect.AdminService.Application.Common.Mappings;

public static class DocumentMappingExtensions
{
    public static DocumentDto ToDto(this Document entity)
    {
        return new DocumentDto
        {
            Id = entity.Id,
            UploaderId = entity.UploaderId,
            Title = entity.Title,
            Description = entity.Description,
            FileUrl = entity.FileUrl,
            FileType = entity.FileType,
            Status = entity.Status,
            Version = entity.Version,
            FileSizeBytes = entity.FileSizeBytes,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            AccessList = entity.AccessList.Select(a => a.ToDto()).ToList()
        };
    }

    public static DocumentAccessDto ToDto(this DocumentAccess access)
    {
        return new DocumentAccessDto
        {
            Id = access.Id,
            DocumentId = access.DocumentId,
            RoleId = access.RoleId,
            DepartmentId = access.DepartmentId
        };
    }
}
