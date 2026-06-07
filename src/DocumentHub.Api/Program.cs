using DocumentHub.Application;
using DocumentHub.Application.Auth;
using DocumentHub.Domain.Exceptions;
using DocumentHub.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplicationHandlers();

var app = builder.Build();

app.InitializeDatabase();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseAuthentication();
app.UseAuthorization();

app.MapPost("/auth/register", async (RegisterRequest request, RegisterHandler handler) =>
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
});

app.Run();

record RegisterRequest(string CompanyName, string Email, string Password);

public partial class Program { }
