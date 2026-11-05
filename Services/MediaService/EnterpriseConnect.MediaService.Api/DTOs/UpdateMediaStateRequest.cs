namespace EnterpriseConnect.MediaService.Api.DTOs;

public record UpdateMediaStateRequest(
    bool? IsMuted,
    bool? IsCameraOn,
    bool? IsScreenSharing);
