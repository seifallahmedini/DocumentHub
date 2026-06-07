using DocumentHub.Core.Exceptions;
using DocumentHub.Core.UseCases.Auth;

namespace DocumentHub.Web.Endpoints.Auth;

public static class RegisterEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapPost("/auth/register", HandleAsync);

    private static async Task<IResult> HandleAsync(RegisterRequest request, RegisterHandler handler)
    {
        try
        {
            var result = await handler.HandleAsync(
                new RegisterCommand(request.CompanyName, request.Email, request.Password));
            return Results.Ok(new { token = result.Token });
        }
        catch (RegistrationValidationException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
        catch (DuplicateEmailException)
        {
            return Results.Conflict(new { error = "A user with this email already exists." });
        }
    }
}

internal record RegisterRequest(string CompanyName, string Email, string Password);
