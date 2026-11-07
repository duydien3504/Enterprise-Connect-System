namespace EnterpriseConnect.ApiGateway.Services;

using System.Text.Json.Nodes;

public class UnifiedSwaggerService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _config;
    private static string? _cachedJson;
    private static DateTimeOffset _cacheExpiry = DateTimeOffset.MinValue;

    public UnifiedSwaggerService(IHttpClientFactory httpClientFactory, IConfiguration config)
    {
        _httpClientFactory = httpClientFactory;
        _config = config;
    }

    public async Task<string> GetUnifiedSwaggerJsonAsync(CancellationToken cancellationToken = default)
    {
        if (_cachedJson != null && DateTimeOffset.UtcNow < _cacheExpiry)
        {
            return _cachedJson;
        }

        var client = _httpClientFactory.CreateClient();
        client.Timeout = TimeSpan.FromSeconds(3);

        var secret = _config["Security:GatewaySharedSecret"] ?? Environment.GetEnvironmentVariable("GATEWAY_SHARED_SECRET");
        if (!string.IsNullOrWhiteSpace(secret))
        {
            client.DefaultRequestHeaders.TryAddWithoutValidation("X-Gateway-Secret", secret);
        }

        var idUrl = _config["ReverseProxy:Clusters:identity-cluster:Destinations:identity-service:Address"] ?? "http://localhost:5223";
        var msgUrl = _config["ReverseProxy:Clusters:messaging-cluster:Destinations:messaging-service:Address"] ?? "http://localhost:5166";
        var medUrl = _config["ReverseProxy:Clusters:media-cluster:Destinations:media-service:Address"] ?? "http://localhost:5116";
        var remUrl = _config["ReverseProxy:Clusters:remote-cluster:Destinations:remote-service:Address"] ?? "http://localhost:5210";
        var admUrl = _config["ReverseProxy:Clusters:admin-cluster:Destinations:admin-service:Address"] ?? "http://localhost:5298";

        var endpoints = new[]
        {
            $"{idUrl.TrimEnd('/')}/swagger/v1/swagger.json",
            $"{msgUrl.TrimEnd('/')}/swagger/v1/swagger.json",
            $"{medUrl.TrimEnd('/')}/swagger/v1/swagger.json",
            $"{remUrl.TrimEnd('/')}/swagger/v1/swagger.json",
            $"{admUrl.TrimEnd('/')}/swagger/v1/swagger.json"
        };

        var root = new JsonObject
        {
            ["swagger"] = "2.0",
            ["info"] = new JsonObject
            {
                ["title"] = "EnterpriseConnect Unified Gateway API",
                ["version"] = "v1",
                ["description"] = "Tất cả endpoint từ các Microservices tập hợp trên API Gateway"
            },
            ["paths"] = new JsonObject(),
            ["definitions"] = new JsonObject(),
            ["securityDefinitions"] = new JsonObject
            {
                ["Bearer"] = new JsonObject
                {
                    ["type"] = "apiKey",
                    ["name"] = "Authorization",
                    ["in"] = "header",
                    ["description"] = "JWT Authorization header. Dán trực tiếp chuỗi token."
                }
            },
            ["security"] = new JsonArray
            {
                new JsonObject { ["Bearer"] = new JsonArray() }
            }
        };

        var pathsNode = root["paths"]!.AsObject();
        var defsNode = root["definitions"]!.AsObject();

        foreach (var url in endpoints)
        {
            try
            {
                var json = await client.GetStringAsync(url, cancellationToken);
                var doc = JsonNode.Parse(json);
                if (doc?["paths"] is JsonObject srcPaths)
                {
                    foreach (var kvp in srcPaths)
                    {
                        if (!pathsNode.ContainsKey(kvp.Key))
                        {
                            pathsNode[kvp.Key] = kvp.Value?.DeepClone();
                        }
                    }
                }
                if (doc?["definitions"] is JsonObject srcDefs)
                {
                    foreach (var kvp in srcDefs)
                    {
                        if (!defsNode.ContainsKey(kvp.Key))
                        {
                            defsNode[kvp.Key] = kvp.Value?.DeepClone();
                        }
                    }
                }
            }
            catch
            {
                // Service offline, ignore gracefully
            }
        }

        _cachedJson = root.ToJsonString();
        _cacheExpiry = DateTimeOffset.UtcNow.AddSeconds(30);
        return _cachedJson;
    }
}
