using System.Text;
using DocumentHub.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace DocumentHub.IntegrationTests;

public class TestWebApplicationFactory : WebApplicationFactory<Program>, IDisposable
{
    private const string TestJwtKey = "test-secret-key-for-testing-only-32chars!!";
    private readonly string _dbPath;

    public TestWebApplicationFactory()
    {
        _dbPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.db");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration(config =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = TestJwtKey,
                ["Jwt:Issuer"] = "DocumentHub"
            });
        });

        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor != null)
                services.Remove(descriptor);

            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlite($"Data Source={_dbPath}"));

            // Ensure JWT validation key matches what JwtTokenService uses for signing
            services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.TokenValidationParameters.IssuerSigningKey =
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TestJwtKey));
            });
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (!disposing) return;
        try { if (File.Exists(_dbPath)) File.Delete(_dbPath); } catch (IOException) { }
        try { if (File.Exists(_dbPath + "-wal")) File.Delete(_dbPath + "-wal"); } catch (IOException) { }
        try { if (File.Exists(_dbPath + "-shm")) File.Delete(_dbPath + "-shm"); } catch (IOException) { }
    }
}
