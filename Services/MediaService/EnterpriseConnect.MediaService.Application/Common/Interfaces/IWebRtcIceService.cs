using EnterpriseConnect.MediaService.Application.Common.Models;

namespace EnterpriseConnect.MediaService.Application.Common.Interfaces;

public interface IWebRtcIceService
{
    IReadOnlyList<IceServerDto> GetIceServers();
}
