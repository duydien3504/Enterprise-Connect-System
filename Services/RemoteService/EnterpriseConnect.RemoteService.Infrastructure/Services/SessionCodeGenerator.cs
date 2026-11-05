using System.Security.Cryptography;
using EnterpriseConnect.RemoteService.Application.Common.Interfaces;

namespace EnterpriseConnect.RemoteService.Infrastructure.Services;

public class SessionCodeGenerator : ISessionCodeGenerator
{
    public string GenerateCode()
    {
        var part1 = RandomNumberGenerator.GetInt32(100, 1000);
        var part2 = RandomNumberGenerator.GetInt32(100, 1000);
        return $"{part1:D3}-{part2:D3}";
    }
}
