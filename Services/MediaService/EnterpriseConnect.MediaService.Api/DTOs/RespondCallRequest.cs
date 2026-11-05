namespace EnterpriseConnect.MediaService.Api.DTOs;

public record RespondCallRequest(
    bool Accept,
    string? DeclineReason);
