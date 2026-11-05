namespace EnterpriseConnect.IdentityService.Application.Features.Contacts.Commands;

public record SendContactResultDto(Guid ContactId, int Status, string Message);
