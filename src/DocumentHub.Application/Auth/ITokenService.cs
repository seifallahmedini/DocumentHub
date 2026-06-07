using DocumentHub.Domain.Entities;

namespace DocumentHub.Application.Auth;

public interface ITokenService
{
    string GenerateToken(User user);
}
