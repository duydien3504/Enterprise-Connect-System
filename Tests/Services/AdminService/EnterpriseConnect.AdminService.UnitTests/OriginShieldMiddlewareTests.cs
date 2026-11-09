using EnterpriseConnect.BuildingBlocks.Infrastructure.Security;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace EnterpriseConnect.AdminService.UnitTests;

public class OriginShieldMiddlewareTests
{
    private const string ExpectedSecret = "TopSecretSharedToken123!";

    private static IConfiguration BuildConfig(string? secret)
    {
        var settings = new Dictionary<string, string?>();
        if (secret != null) settings["Security:GatewaySharedSecret"] = secret;
        return new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
    }

    private static OriginShieldMiddleware CreateMiddleware(RequestDelegate next, IConfiguration config) =>
        new(next, config, Mock.Of<ILogger<OriginShieldMiddleware>>());

    [Fact]
    public async Task InvokeAsync_WhenSecretNotConfigured_AllowsRequest()
    {
        var nextInvoked = false;
        var middleware = CreateMiddleware(_ => { nextInvoked = true; return Task.CompletedTask; }, BuildConfig(null));
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/v1/test";

        await middleware.InvokeAsync(context);

        nextInvoked.Should().BeTrue();
        context.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
    }

    [Theory]
    [InlineData("/")]
    [InlineData("/health")]
    [InlineData("/health/live")]
    [InlineData("/health/ready")]
    [InlineData("/swagger")]
    [InlineData("/swagger/v1/swagger.json")]
    [InlineData("/swagger/index.html")]
    public async Task InvokeAsync_WhenPathIsBypassed_AllowsRequestWithoutHeader(string path)
    {
        var nextInvoked = false;
        var middleware = CreateMiddleware(_ => { nextInvoked = true; return Task.CompletedTask; }, BuildConfig(ExpectedSecret));
        var context = new DefaultHttpContext();
        context.Request.Path = path;

        await middleware.InvokeAsync(context);

        nextInvoked.Should().BeTrue();
        context.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
    }

    [Fact]
    public async Task InvokeAsync_WhenSecretConfiguredAndHeaderMissing_Returns403()
    {
        var nextInvoked = false;
        var middleware = CreateMiddleware(_ => { nextInvoked = true; return Task.CompletedTask; }, BuildConfig(ExpectedSecret));
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/v1/protected";

        await middleware.InvokeAsync(context);

        nextInvoked.Should().BeFalse();
        context.Response.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
    }

    [Fact]
    public async Task InvokeAsync_WhenHeaderWrong_Returns403()
    {
        var nextInvoked = false;
        var middleware = CreateMiddleware(_ => { nextInvoked = true; return Task.CompletedTask; }, BuildConfig(ExpectedSecret));
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/v1/protected";
        context.Request.Headers["X-Gateway-Secret"] = "WrongSecret!";

        await middleware.InvokeAsync(context);

        nextInvoked.Should().BeFalse();
        context.Response.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
    }

    [Fact]
    public async Task InvokeAsync_WhenHeaderCorrect_CallsNext()
    {
        var nextInvoked = false;
        var middleware = CreateMiddleware(_ => { nextInvoked = true; return Task.CompletedTask; }, BuildConfig(ExpectedSecret));
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/v1/protected";
        context.Request.Headers["X-Gateway-Secret"] = ExpectedSecret;

        await middleware.InvokeAsync(context);

        nextInvoked.Should().BeTrue();
        context.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
    }

    [Fact]
    public async Task InvokeAsync_WhenFromGatewayIp_AllowsRequestWithoutSecret()
    {
        var nextInvoked = false;
        var middleware = CreateMiddleware(_ => { nextInvoked = true; return Task.CompletedTask; }, BuildConfig(ExpectedSecret));
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/v1/protected";
        context.Request.Headers["X-Forwarded-For"] = "13.238.207.194";

        await middleware.InvokeAsync(context);

        nextInvoked.Should().BeTrue();
        context.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
    }
}
