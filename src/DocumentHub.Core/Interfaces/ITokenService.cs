namespace DocumentHub.Core.Interfaces;

public interface ITokenService
{
    string GenerateToken(Guid userId, Guid tenantId, string role);
}
