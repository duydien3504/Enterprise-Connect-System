using EnterpriseConnect.MediaService.Application.Common.Interfaces;
using EnterpriseConnect.MediaService.Application.Common.Models;
using MediatR;

namespace EnterpriseConnect.MediaService.Application.Features.Calls.Queries.GetIceServers;

public class GetIceServersQueryHandler : IRequestHandler<GetIceServersQuery, Result<IReadOnlyList<IceServerDto>>>
{
    private readonly IWebRtcIceService _iceService;

    public GetIceServersQueryHandler(IWebRtcIceService iceService)
    {
        _iceService = iceService;
    }

    public Task<Result<IReadOnlyList<IceServerDto>>> Handle(
        GetIceServersQuery request,
        CancellationToken cancellationToken)
    {
        var servers = _iceService.GetIceServers();
        return Task.FromResult(Result<IReadOnlyList<IceServerDto>>.Success(servers));
    }
}
