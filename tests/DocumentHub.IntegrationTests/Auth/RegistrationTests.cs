using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;

namespace DocumentHub.IntegrationTests.Auth;

public class RegistrationTests(TestWebApplicationFactory factory)
    : IClassFixture<TestWebApplicationFactory>
{
    [Fact]
    public async Task Register_WithValidData_CreatesTenantAndAdminAndReturnsJwt()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/auth/register", new
        {
            companyName = "TitanCore SUARL",
            email = "admin@titancore.tn",
            password = "SecurePass123!"
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("token").GetString().Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_Returns409()
    {
        var client = factory.CreateClient();
        var request = new
        {
            companyName = "Acme Corp",
            email = "duplicate@example.com",
            password = "SecurePass123!"
        };

        await client.PostAsJsonAsync("/auth/register", request);
        var response = await client.PostAsJsonAsync("/auth/register", request);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Register_WithMissingFields_Returns400()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/auth/register", new
        {
            companyName = "",
            email = "",
            password = ""
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
