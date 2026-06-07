using DocumentHub.Core.Entities;
using DocumentHub.Core.Exceptions;
using DocumentHub.Core.Interfaces;

namespace DocumentHub.Core.UseCases.Auth;

public record LoginCommand(string Email, string Password);
public record LoginResult(string Token);

public class LoginHandler(IRepository<User> users, ITokenService tokenService, IPasswordHasher passwordHasher)
{
    public async Task<LoginResult> HandleAsync(LoginCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.Email) || string.IsNullOrWhiteSpace(command.Password))
            throw new RegistrationValidationException("Email and Password are required.");

        var email = command.Email.Trim().ToLower();
        var user = await users.FirstOrDefaultAsync(u => u.Email == email);

        if (user is null || !passwordHasher.Verify(command.Password, user.PasswordHash))
            throw new InvalidCredentialsException();

        return new LoginResult(tokenService.GenerateToken(user.Id, user.TenantId, user.Role.ToString()));
    }
}
