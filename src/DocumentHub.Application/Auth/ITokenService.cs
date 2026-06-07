namespace DocumentHub.Application.Auth;

public interface ITokenService
{
    string GenerateToken(Guid userId, Guid tenantId, string role);
}
