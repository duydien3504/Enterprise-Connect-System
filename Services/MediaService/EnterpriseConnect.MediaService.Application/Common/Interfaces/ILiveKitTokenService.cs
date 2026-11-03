namespace EnterpriseConnect.MediaService.Application.Common.Interfaces;

public interface ILiveKitTokenService
{
    string GenerateJoinToken(string roomName, string identity, string name, bool canPublish = true, bool canSubscribe = true);
}
