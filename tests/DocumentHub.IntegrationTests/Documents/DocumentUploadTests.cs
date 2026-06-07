using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using FluentAssertions;

namespace DocumentHub.IntegrationTests.Documents;

public class DocumentUploadTests(TestWebApplicationFactory factory) : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private async Task<string> LoginAsAdminAsync(string company, string email)
    {
        await _client.PostAsJsonAsync("/auth/register", new { companyName = company, email, password = "SecurePass123!" });
        var res = await _client.PostAsJsonAsync("/auth/login", new { email, password = "SecurePass123!" });
        var body = await res.Content.ReadFromJsonAsync<JsonElement>();
        return body.GetProperty("token").GetString()!;
    }

    private void Authorize(string token) =>
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

    private static MultipartFormDataContent BuildUpload(string name, string tags = "", string content = "hello world", string fileName = "test.txt")
    {
        var form = new MultipartFormDataContent();
        form.Add(new StringContent(name), "name");
        if (!string.IsNullOrEmpty(tags)) form.Add(new StringContent(tags), "tags");
        var fileContent = new ByteArrayContent(Encoding.UTF8.GetBytes(content));
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("text/plain");
        form.Add(fileContent, "file", fileName);
        return form;
    }

    [Fact]
    public async Task Upload_WithValidFile_Returns201WithId()
    {
        var token = await LoginAsAdminAsync("UpCorp1", "up1@example.com");
        Authorize(token);

        var response = await _client.PostAsync("/documents", BuildUpload("My Doc", "invoice,2025"));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("id").GetString().Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Download_ExistingDocument_ReturnsFileContent()
    {
        var token = await LoginAsAdminAsync("UpCorp2", "up2@example.com");
        Authorize(token);

        var uploadRes = await _client.PostAsync("/documents", BuildUpload("Download Me", content: "file-content-123"));
        var uploadBody = await uploadRes.Content.ReadFromJsonAsync<JsonElement>();
        var docId = uploadBody.GetProperty("id").GetString();

        var downloadRes = await _client.GetAsync($"/documents/{docId}/download");

        downloadRes.StatusCode.Should().Be(HttpStatusCode.OK);
        var bytes = await downloadRes.Content.ReadAsByteArrayAsync();
        Encoding.UTF8.GetString(bytes).Should().Be("file-content-123");
    }

    [Fact]
    public async Task Download_NonExistentDocument_Returns404()
    {
        var token = await LoginAsAdminAsync("UpCorp3", "up3@example.com");
        Authorize(token);

        var response = await _client.GetAsync($"/documents/{Guid.NewGuid()}/download");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Download_DocumentFromOtherTenant_Returns404()
    {
        var token1 = await LoginAsAdminAsync("UpCorp4a", "up4a@example.com");
        Authorize(token1);
        var uploadRes = await _client.PostAsync("/documents", BuildUpload("Tenant A Doc"));
        var uploadBody = await uploadRes.Content.ReadFromJsonAsync<JsonElement>();
        var docId = uploadBody.GetProperty("id").GetString();

        var token2 = await LoginAsAdminAsync("UpCorp4b", "up4b@example.com");
        Authorize(token2);

        var response = await _client.GetAsync($"/documents/{docId}/download");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Update_ExistingDocument_Returns204()
    {
        var token = await LoginAsAdminAsync("UpCorp5", "up5@example.com");
        Authorize(token);

        var uploadRes = await _client.PostAsync("/documents", BuildUpload("Original Name", "tag1"));
        var uploadBody = await uploadRes.Content.ReadFromJsonAsync<JsonElement>();
        var docId = uploadBody.GetProperty("id").GetString();

        var response = await _client.PatchAsJsonAsync($"/documents/{docId}",
            new { name = "Updated Name", tags = new[] { "tag2", "tag3" } });

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Update_NonExistentDocument_Returns404()
    {
        var token = await LoginAsAdminAsync("UpCorp6", "up6@example.com");
        Authorize(token);

        var response = await _client.PatchAsJsonAsync($"/documents/{Guid.NewGuid()}",
            new { name = "X", tags = Array.Empty<string>() });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_ExistingDocument_Returns204()
    {
        var token = await LoginAsAdminAsync("UpCorp7", "up7@example.com");
        Authorize(token);

        var uploadRes = await _client.PostAsync("/documents", BuildUpload("To Delete"));
        var uploadBody = await uploadRes.Content.ReadFromJsonAsync<JsonElement>();
        var docId = uploadBody.GetProperty("id").GetString();

        var response = await _client.DeleteAsync($"/documents/{docId}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Delete_NonExistentDocument_Returns404()
    {
        var token = await LoginAsAdminAsync("UpCorp8", "up8@example.com");
        Authorize(token);

        var response = await _client.DeleteAsync($"/documents/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Upload_Unauthenticated_Returns401()
    {
        _client.DefaultRequestHeaders.Authorization = null;
        var response = await _client.PostAsync("/documents", BuildUpload("No Auth"));
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
