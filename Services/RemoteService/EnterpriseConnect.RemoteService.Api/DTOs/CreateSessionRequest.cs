namespace EnterpriseConnect.RemoteService.Api.DTOs;

public record CreateSessionRequest(
    int ExpirationMinutes = 15
);
