using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;

namespace DocumentHub.IntegrationTests.Users;

public class MemberInvitationTests(TestWebApplicationFactory factory) : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private async Task<string> RegisterAndLoginAsync(string companyName, string email, string password = "SecurePass123!")
    {
        await _client.PostAsJsonAsync("/auth/register", new { companyName, email, password });
        var res = await _client.PostAsJsonAsync("/auth/login", new { email, password });
        var body = await res.Content.ReadFromJsonAsync<JsonElement>();
        return body.GetProperty("token").GetString()!;
    }

    private void Authorize(string token) =>
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

    private void ClearAuth() =>
        _client.DefaultRequestHeaders.Authorization = null;

    [Fact]
    public async Task InviteMember_AsAdmin_Returns201WithMemberId()
    {
        var adminToken = await RegisterAndLoginAsync("Alpha Corp", "alpha-admin@example.com");
        Authorize(adminToken);

        var response = await _client.PostAsJsonAsync("/users/invite", new
        {
            email = "alpha-member@example.com",
            password = "MemberPass123!"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("id").GetString().Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task InviteMember_WithDuplicateEmail_Returns409()
    {
        var adminToken = await RegisterAndLoginAsync("Beta Corp", "beta-admin@example.com");
        Authorize(adminToken);

        await _client.PostAsJsonAsync("/users/invite", new
        {
            email = "beta-member@example.com",
            password = "MemberPass123!"
        });

        var response = await _client.PostAsJsonAsync("/users/invite", new
        {
            email = "beta-member@example.com",
            password = "MemberPass123!"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task InviteMember_AsMember_Returns403()
    {
        var adminToken = await RegisterAndLoginAsync("Gamma Corp", "gamma-admin@example.com");
        Authorize(adminToken);
        await _client.PostAsJsonAsync("/users/invite", new
        {
            email = "gamma-member@example.com",
            password = "MemberPass123!"
        });
        ClearAuth();

        var memberToken = await RegisterAndLoginAsync("Gamma Corp 2", "gamma-member2@example.com");
        // gamma-member2 is an Admin of a different tenant; we need a real Member token
        // So: login as gamma-member (was invited, so is a Member in Gamma Corp)
        var loginRes = await _client.PostAsJsonAsync("/auth/login", new
        {
            email = "gamma-member@example.com",
            password = "MemberPass123!"
        });
        var memberBody = await loginRes.Content.ReadFromJsonAsync<JsonElement>();
        var realMemberToken = memberBody.GetProperty("token").GetString()!;
        Authorize(realMemberToken);

        var response = await _client.PostAsJsonAsync("/users/invite", new
        {
            email = "gamma-member3@example.com",
            password = "MemberPass123!"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task InviteMember_Unauthenticated_Returns401()
    {
        ClearAuth();
        var response = await _client.PostAsJsonAsync("/users/invite", new
        {
            email = "anon@example.com",
            password = "MemberPass123!"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetMembers_ReturnsOnlyMembersInSameTenant()
    {
        var admin1Token = await RegisterAndLoginAsync("Delta Corp", "delta-admin@example.com");
        Authorize(admin1Token);
        await _client.PostAsJsonAsync("/users/invite", new
        {
            email = "delta-member@example.com",
            password = "MemberPass123!"
        });

        var response = await _client.GetAsync("/users");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        var users = body.EnumerateArray().ToList();
        users.Should().HaveCount(2); // admin + member
        users.Select(u => u.GetProperty("email").GetString()).Should().Contain("delta-admin@example.com");
        users.Select(u => u.GetProperty("email").GetString()).Should().Contain("delta-member@example.com");
    }

    [Fact]
    public async Task GetMembers_DoesNotReturnUsersFromOtherTenants()
    {
        var admin1Token = await RegisterAndLoginAsync("Epsilon Corp", "epsilon-admin@example.com");
        await RegisterAndLoginAsync("Zeta Corp", "zeta-admin@example.com");

        Authorize(admin1Token);
        var response = await _client.GetAsync("/users");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        var emails = body.EnumerateArray().Select(u => u.GetProperty("email").GetString()).ToList();
        emails.Should().NotContain("zeta-admin@example.com");
    }

    [Fact]
    public async Task RemoveMember_AsAdmin_Returns204()
    {
        var adminToken = await RegisterAndLoginAsync("Eta Corp", "eta-admin@example.com");
        Authorize(adminToken);
        var inviteRes = await _client.PostAsJsonAsync("/users/invite", new
        {
            email = "eta-member@example.com",
            password = "MemberPass123!"
        });
        var inviteBody = await inviteRes.Content.ReadFromJsonAsync<JsonElement>();
        var memberId = inviteBody.GetProperty("id").GetString();

        var response = await _client.DeleteAsync($"/users/{memberId}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task RemoveMember_AdminCannotRemoveThemselves_Returns400()
    {
        var adminToken = await RegisterAndLoginAsync("Theta Corp", "theta-admin@example.com");
        Authorize(adminToken);

        // Get users to find admin's own ID
        var usersRes = await _client.GetAsync("/users");
        var users = await usersRes.Content.ReadFromJsonAsync<JsonElement>();
        var adminUser = users.EnumerateArray().First(u => u.GetProperty("email").GetString() == "theta-admin@example.com");
        var adminId = adminUser.GetProperty("id").GetString();

        var response = await _client.DeleteAsync($"/users/{adminId}");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RemoveMember_AsMember_Returns403()
    {
        var adminToken = await RegisterAndLoginAsync("Iota Corp", "iota-admin@example.com");
        Authorize(adminToken);
        var inviteRes = await _client.PostAsJsonAsync("/users/invite", new
        {
            email = "iota-member@example.com",
            password = "MemberPass123!"
        });
        var inviteBody = await inviteRes.Content.ReadFromJsonAsync<JsonElement>();
        var memberId = inviteBody.GetProperty("id").GetString();
        ClearAuth();

        var loginRes = await _client.PostAsJsonAsync("/auth/login", new
        {
            email = "iota-member@example.com",
            password = "MemberPass123!"
        });
        var memberBody = await loginRes.Content.ReadFromJsonAsync<JsonElement>();
        Authorize(memberBody.GetProperty("token").GetString()!);

        var response = await _client.DeleteAsync($"/users/{memberId}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task RemoveMember_NonExistentUser_Returns404()
    {
        var adminToken = await RegisterAndLoginAsync("Kappa Corp", "kappa-admin@example.com");
        Authorize(adminToken);

        var response = await _client.DeleteAsync($"/users/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
