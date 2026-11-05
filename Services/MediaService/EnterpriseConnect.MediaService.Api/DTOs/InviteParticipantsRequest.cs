namespace EnterpriseConnect.MediaService.Api.DTOs;

public record InviteParticipantsRequest(
    List<Guid> UserIds);
