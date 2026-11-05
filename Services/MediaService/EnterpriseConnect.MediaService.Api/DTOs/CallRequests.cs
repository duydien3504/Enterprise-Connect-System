using EnterpriseConnect.MediaService.Domain.Enums;

namespace EnterpriseConnect.MediaService.Api.DTOs;

public record InitiateCallRequest(
    Guid ConversationId,
    Guid InitiatorId,
    CallType CallType,
    CallScope Scope,
    List<Guid> ParticipantIds);

public record RespondCallRequest(
    Guid UserId,
    bool Accept,
    string? DeclineReason);

public record EndCallRequest(
    Guid UserId,
    string? Reason);

public record UpdateMediaStateRequest(
    Guid UserId,
    bool? IsMuted,
    bool? IsCameraOn,
    bool? IsScreenSharing);

public record InviteParticipantsRequest(
    Guid InviterId,
    List<Guid> UserIds);
