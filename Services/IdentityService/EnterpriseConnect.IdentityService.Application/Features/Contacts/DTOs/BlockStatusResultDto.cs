namespace EnterpriseConnect.IdentityService.Application.Features.Contacts.Queries;

public record BlockStatusResultDto(bool IsBlocked, Guid? BlockedBy);
