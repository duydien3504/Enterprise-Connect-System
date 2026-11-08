using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EnterpriseConnect.MediaService.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace EnterpriseConnect.MediaService.Infrastructure.Services;

public class LiveKitTokenService : ILiveKitTokenService
{
    private readonly string _apiKey;
    private readonly string _apiSecret;

    public LiveKitTokenService(IConfiguration configuration)
    {
        _apiKey = configuration["LiveKit:ApiKey"]
            ?? Environment.GetEnvironmentVariable("LiveKit__ApiKey")
            ?? "devkey";
        _apiSecret = configuration["LiveKit:ApiSecret"]
            ?? Environment.GetEnvironmentVariable("LiveKit__ApiSecret")
            ?? string.Empty;
    }

    public string GenerateJoinToken(string roomName, string identity, string name, bool canPublish = true, bool canSubscribe = true)
    {
        if (string.IsNullOrWhiteSpace(_apiSecret))
        {
            throw new InvalidOperationException("LiveKit: Cấu hình 'LiveKit:ApiSecret' không được để trống khi tạo Join Token.");
        }
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_apiSecret);

        var claims = new List<Claim>
        {
            new Claim("sub", identity),
            new Claim("iss", _apiKey),
            new Claim("name", name),
            new Claim("room", roomName)
        };

        var videoGrants = new Dictionary<string, object>
        {
            { "roomJoin", true },
            { "room", roomName },
            { "canPublish", canPublish },
            { "canSubscribe", canSubscribe }
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(4),
            Issuer = _apiKey,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
            Claims = new Dictionary<string, object>
            {
                { "video", videoGrants }
            }
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
