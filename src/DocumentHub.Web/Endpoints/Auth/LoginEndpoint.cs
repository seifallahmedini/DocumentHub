using DocumentHub.Core.Exceptions;
using DocumentHub.Core.UseCases.Auth;

namespace DocumentHub.Web.Endpoints.Auth;

public static class LoginEndpoint
{
    public static IEndpointRouteBuilder MapLoginEndpoint(this IEndpointRouteBuilder group)
    {
        group.MapPost("/login", HandleAsync);
        return group;
    }

    private static async Task<IResult> HandleAsync(LoginRequest request, LoginHandler handler)
    {
        try
        {
            var result = await handler.HandleAsync(
                new LoginCommand(request.Email, request.Password));
            return Results.Ok(new { token = result.Token });
        }
        catch (RegistrationValidationException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
        catch (InvalidCredentialsException)
        {
            return Results.Unauthorized();
        }
    }
}

internal record LoginRequest(string Email, string Password);
