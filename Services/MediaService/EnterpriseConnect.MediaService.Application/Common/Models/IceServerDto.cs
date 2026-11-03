namespace EnterpriseConnect.MediaService.Application.Common.Models;

public class IceServerDto
{
    public string[] Urls { get; set; } = Array.Empty<string>();
    public string? Username { get; set; }
    public string? Credential { get; set; }
}
