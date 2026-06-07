using DocumentHub.Core.Exceptions;
using DocumentHub.Core.UseCases.Users;

namespace DocumentHub.Web.Endpoints.Users;

public static class UsersEndpoint
{
    public static IEndpointRouteBuilder MapUsersEndpoints(this IEndpointRouteBuilder group)
    {
        group.MapPost("/invite", InviteAsync);
        group.MapDelete("/{id:guid}", RemoveAsync);
        group.MapGet("/", GetMembersAsync);
        return group;
    }

    private static async Task<IResult> InviteAsync(
        InviteRequest request,
        InviteHandler handler,
        HttpContext ctx)
    {
        var (tenantId, role) = ExtractClaims(ctx);
        try
        {
            var result = await handler.HandleAsync(new InviteCommand(request.Email, request.Password, tenantId, role));
            return Results.Created($"/users/{result.Id}", new { id = result.Id.ToString() });
        }
        catch (ForbiddenException ex)
        {
            return Results.Problem(ex.Message, statusCode: StatusCodes.Status403Forbidden);
        }
        catch (DuplicateEmailException)
        {
            return Results.Conflict(new { error = "A user with this email already exists." });
        }
    }

    private static async Task<IResult> RemoveAsync(
        Guid id,
        RemoveMemberHandler handler,
        HttpContext ctx)
    {
        var (tenantId, role) = ExtractClaims(ctx);
        var callerId = Guid.Parse(ctx.User.FindFirst("userId")!.Value);
        try
        {
            await handler.HandleAsync(new RemoveMemberCommand(id, callerId, tenantId, role));
            return Results.NoContent();
        }
        catch (ForbiddenException ex)
        {
            return Results.Problem(ex.Message, statusCode: StatusCodes.Status403Forbidden);
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
        catch (NotFoundException ex)
        {
            return Results.NotFound(new { error = ex.Message });
        }
    }

    private static async Task<IResult> GetMembersAsync(
        GetMembersHandler handler,
        HttpContext ctx)
    {
        var (tenantId, _) = ExtractClaims(ctx);
        var members = await handler.HandleAsync(new GetMembersQuery(tenantId));
        return Results.Ok(members);
    }

    private static (Guid tenantId, string role) ExtractClaims(HttpContext ctx)
    {
        var tenantId = Guid.Parse(ctx.User.FindFirst("tenantId")!.Value);
        var role = ctx.User.FindFirst("role")!.Value;
        return (tenantId, role);
    }
}

internal record InviteRequest(string Email, string Password);
