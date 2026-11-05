namespace EnterpriseConnect.RemoteService.Api.DTOs;

public record ApproveSessionRequest(
    bool Accept,
    string? RejectionReason = null
);
