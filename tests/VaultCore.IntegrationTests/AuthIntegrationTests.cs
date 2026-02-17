using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using VaultCore.Application.DTOs.Auth;
using Xunit;

namespace VaultCore.IntegrationTests;

/// <summary>
/// Integration tests for auth: register and login.
/// Uses WebApplicationFactory and in-memory DB (configure in test host).
/// </summary>
public class AuthIntegrationTests : IClassFixture<WebApplicationFactoryFixture>
{
    private readonly HttpClient _client;

    public AuthIntegrationTests(WebApplicationFactoryFixture factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_WithValidRequest_Returns200AndTokens()
    {
        var email = $"test_{Guid.NewGuid():N}@example.com";
        var request = new RegisterRequest(email, "Pass@word123", "Test", "User", null);
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.NotNull(body);
        Assert.True(body.TryGetProperty("accessToken", out _));
        Assert.True(body.TryGetProperty("refreshToken", out _));
        Assert.True(body.TryGetProperty("user", out _));
    }

    [Fact]
    public async Task Register_WithInvalidEmail_Returns400()
    {
        var request = new RegisterRequest("not-an-email", "Pass@word123", "A", "B", null);
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithWrongPassword_Returns401()
    {
        var request = new LoginRequest("admin@vaultcore.local", "WrongPass");
        var response = await _client.PostAsJsonAsync("/api/auth/login", request);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
