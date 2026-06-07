using DocumentHub.Core.Exceptions;
using DocumentHub.Core.UseCases.Documents;

namespace DocumentHub.Web.Endpoints.Documents;

public static class DocumentsEndpoint
{
    public static IEndpointRouteBuilder MapDocumentsEndpoints(this IEndpointRouteBuilder group)
    {
        group.MapPost("/", UploadAsync).DisableAntiforgery();
        group.MapGet("/{id:guid}/download", DownloadAsync);
        group.MapPatch("/{id:guid}", UpdateAsync);
        group.MapDelete("/{id:guid}", DeleteAsync);
        return group;
    }

    private static async Task<IResult> UploadAsync(
        IFormFile file,
        [Microsoft.AspNetCore.Mvc.FromForm] string name,
        [Microsoft.AspNetCore.Mvc.FromForm] string? tags,
        UploadDocumentHandler handler,
        HttpContext ctx)
    {
        var (userId, tenantId, _) = ExtractClaims(ctx);
        var tagList = ParseTags(tags);

        await using var stream = file.OpenReadStream();
        var result = await handler.HandleAsync(new UploadDocumentCommand(
            name,
            tagList,
            file.FileName,
            file.ContentType,
            stream,
            file.Length,
            userId,
            tenantId));

        return Results.Created($"/documents/{result.Id}/download", new { id = result.Id });
    }

    private static async Task<IResult> DownloadAsync(
        Guid id,
        DownloadDocumentHandler handler,
        HttpContext ctx)
    {
        var (_, tenantId, _) = ExtractClaims(ctx);
        try
        {
            var result = await handler.HandleAsync(new DownloadDocumentQuery(id, tenantId));
            return Results.File(result.Content, result.MimeType, result.FileName);
        }
        catch (NotFoundException ex)
        {
            return Results.NotFound(new { error = ex.Message });
        }
    }

    private static async Task<IResult> UpdateAsync(
        Guid id,
        UpdateDocumentRequest request,
        UpdateDocumentHandler handler,
        HttpContext ctx)
    {
        var (_, tenantId, _) = ExtractClaims(ctx);
        try
        {
            await handler.HandleAsync(new UpdateDocumentCommand(id, request.Name, request.Tags, tenantId));
            return Results.NoContent();
        }
        catch (NotFoundException ex)
        {
            return Results.NotFound(new { error = ex.Message });
        }
    }

    private static async Task<IResult> DeleteAsync(
        Guid id,
        DeleteDocumentHandler handler,
        HttpContext ctx)
    {
        var (_, tenantId, _) = ExtractClaims(ctx);
        try
        {
            await handler.HandleAsync(new DeleteDocumentCommand(id, tenantId));
            return Results.NoContent();
        }
        catch (NotFoundException ex)
        {
            return Results.NotFound(new { error = ex.Message });
        }
    }

    private static (Guid userId, Guid tenantId, string role) ExtractClaims(HttpContext ctx)
    {
        var userId = Guid.Parse(ctx.User.FindFirst("userId")!.Value);
        var tenantId = Guid.Parse(ctx.User.FindFirst("tenantId")!.Value);
        var role = ctx.User.FindFirst("role")!.Value;
        return (userId, tenantId, role);
    }

    private static List<string> ParseTags(string? tags) =>
        string.IsNullOrWhiteSpace(tags)
            ? []
            : [.. tags.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)];
}

internal record UpdateDocumentRequest(string Name, List<string> Tags);
