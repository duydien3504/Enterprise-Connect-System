using EnterpriseConnect.MediaService.Domain.Enums;

namespace EnterpriseConnect.MediaService.Api.DTOs;

public record InitiateCallRequest(
    Guid ConversationId,
    CallType CallType,
    CallScope Scope,
    List<Guid> ParticipantIds);
