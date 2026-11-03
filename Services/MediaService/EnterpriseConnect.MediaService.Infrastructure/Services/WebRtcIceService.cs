using EnterpriseConnect.MediaService.Application.Common.Interfaces;
using EnterpriseConnect.MediaService.Application.Common.Models;
using Microsoft.Extensions.Configuration;

namespace EnterpriseConnect.MediaService.Infrastructure.Services;

public class WebRtcIceService : IWebRtcIceService
{
    private readonly IConfiguration _configuration;

    public WebRtcIceService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public IReadOnlyList<IceServerDto> GetIceServers()
    {
        var list = new List<IceServerDto>();

        // Default Google STUN servers
        list.Add(new IceServerDto
        {
            Urls = new[] { "stun:stun.l.google.com:19302", "stun:stun1.l.google.com:19302" }
        });

        // Configured TURN server if available
        var turnUrls = _configuration["WebRtc:Turn:Urls"];
        if (!string.IsNullOrEmpty(turnUrls))
        {
            list.Add(new IceServerDto
            {
                Urls = turnUrls.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries),
                Username = _configuration["WebRtc:Turn:Username"],
                Credential = _configuration["WebRtc:Turn:Password"]
            });
        }

        return list;
    }
}
