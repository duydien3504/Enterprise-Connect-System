namespace EnterpriseConnect.RemoteService.Api.DTOs;

public record TerminateSessionRequest(
    string? Reason = null
);
