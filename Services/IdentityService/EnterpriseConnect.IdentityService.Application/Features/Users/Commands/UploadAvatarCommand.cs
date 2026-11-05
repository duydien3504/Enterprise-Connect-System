using MediatR;

namespace EnterpriseConnect.IdentityService.Application.Features.Users.Commands;

public record UploadAvatarCommand(
    Guid UserId,
    Stream FileStream,
    string FileName
) : IRequest<string>;
