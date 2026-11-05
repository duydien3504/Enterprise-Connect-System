using EnterpriseConnect.MediaService.Application.Common.Models;
using MediatR;

namespace EnterpriseConnect.MediaService.Application.Features.Calls.Queries.GetIceServers;

public record GetIceServersQuery() : IRequest<Result<IReadOnlyList<IceServerDto>>>;
