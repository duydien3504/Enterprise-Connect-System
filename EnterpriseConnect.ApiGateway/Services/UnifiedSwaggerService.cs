namespace EnterpriseConnect.ApiGateway.Services;

using System.Text.Json.Nodes;

public class UnifiedSwaggerService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private static string? _cachedJson;
    private static DateTimeOffset _cacheExpiry = DateTimeOffset.MinValue;

    public UnifiedSwaggerService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<string> GetUnifiedSwaggerJsonAsync(CancellationToken cancellationToken = default)
    {
        if (_cachedJson != null && DateTimeOffset.UtcNow < _cacheExpiry)
        {
            return _cachedJson;
        }

        var client = _httpClientFactory.CreateClient();
        client.Timeout = TimeSpan.FromSeconds(3);

        var endpoints = new[]
        {
            "http://localhost:5223/swagger/v1/swagger.json",
            "http://localhost:5166/swagger/v1/swagger.json",
            "http://localhost:5116/swagger/v1/swagger.json",
            "http://localhost:5210/swagger/v1/swagger.json",
            "http://localhost:5298/swagger/v1/swagger.json"
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
