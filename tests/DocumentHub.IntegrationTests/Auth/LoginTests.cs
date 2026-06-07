using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;

namespace DocumentHub.IntegrationTests.Auth;

public class LoginTests(TestWebApplicationFactory factory) : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private async Task RegisterAsync(string email, string password = "SecurePass123!")
    {
        await _client.PostAsJsonAsync("/auth/register", new
        {
            companyName = "Test Corp",
            email,
            password
        });
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsJwt()
    {
        await RegisterAsync("login-valid@example.com");

        var response = await _client.PostAsJsonAsync("/auth/login", new
        {
            email = "login-valid@example.com",
            password = "SecurePass123!"
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("token").GetString().Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_WithWrongPassword_Returns401()
    {
        await RegisterAsync("login-wrongpw@example.com");

        var response = await _client.PostAsJsonAsync("/auth/login", new
        {
            email = "login-wrongpw@example.com",
            password = "WrongPassword!"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WithUnknownEmail_Returns401()
    {
        var response = await _client.PostAsJsonAsync("/auth/login", new
        {
            email = "nobody@example.com",
            password = "SecurePass123!"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WithMissingFields_Returns400()
    {
        var response = await _client.PostAsJsonAsync("/auth/login", new
        {
            email = "",
            password = ""
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
