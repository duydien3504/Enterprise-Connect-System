using EnterpriseConnect.IdentityService.Application.Common.Interfaces;
using MediatR;

namespace EnterpriseConnect.IdentityService.Application.Features.Users.Commands;

public record UpdateProfileCommand(
    Guid UserId,
    string FullName,
    string? Phone,
    string? Position
) : IRequest<bool>;

public class UpdateProfileCommandHandler : IRequestHandler<UpdateProfileCommand, bool>
{
    private readonly IIdentityUnitOfWork _uow;

    public UpdateProfileCommandHandler(IIdentityUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<bool> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        var user = await _uow.Users.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new KeyNotFoundException("Không tìm thấy người dùng.");

        user.FullName = request.FullName.Trim();
        user.Phone = request.Phone?.Trim();
        user.Position = request.Position?.Trim();

        await _uow.Users.UpdateAsync(user, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        return true;
    }
}

public record UploadAvatarCommand(
    Guid UserId,
    Stream FileStream,
    string FileName
) : IRequest<string>;

public class UploadAvatarCommandHandler : IRequestHandler<UploadAvatarCommand, string>
{
    private readonly IIdentityUnitOfWork _uow;
    private readonly IImageValidator _imageValidator;
    private readonly ICloudinaryStorageService _cloudinaryService;

    public UploadAvatarCommandHandler(
        IIdentityUnitOfWork uow,
        IImageValidator imageValidator,
        ICloudinaryStorageService cloudinaryService)
    {
        _uow = uow;
        _imageValidator = imageValidator;
        _cloudinaryService = cloudinaryService;
    }

    public async Task<string> Handle(UploadAvatarCommand request, CancellationToken cancellationToken)
    {
        var user = await _uow.Users.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new KeyNotFoundException("Không tìm thấy người dùng.");

        // 1. Validate & Sanitize ảnh với SixLabors.ImageSharp (Magic bytes, max 5MB, 4096px, resize 500x500 JPEG)
        var sanitizedBytes = await _imageValidator.ValidateAndSanitizeAvatarAsync(request.FileStream, request.FileName, cancellationToken);

        // 2. Upload stream ảnh sạch lên Cloudinary
        using var sanitizedStream = new MemoryStream(sanitizedBytes);
        var avatarUrl = await _cloudinaryService.UploadImageAsync(
            sanitizedStream,
            $"{user.Id}_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}.jpg",
            "avatars",
            cancellationToken);

        // 3. Cập nhật AvatarUrl vào User
        user.AvatarUrl = avatarUrl;
        await _uow.Users.UpdateAsync(user, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        return avatarUrl;
    }
}
