using DocumentHub.Core.Interfaces;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;

namespace DocumentHub.Infrastructure.Storage;

public class LocalFileStore(IConfiguration configuration) : IFileStore
{
    private readonly string _root = configuration["FileStorage:Root"] ?? "uploads";
    private static readonly FileExtensionContentTypeProvider _contentTypeProvider = new();

    public async Task<string> SaveAsync(Guid tenantId, string fileName, Stream content, CancellationToken cancellationToken = default)
    {
        var dir = Path.Combine(_root, tenantId.ToString());
        Directory.CreateDirectory(dir);

        var safeFileName = $"{Guid.NewGuid()}_{Path.GetFileName(fileName)}";
        var filePath = Path.Combine(dir, safeFileName);

        await using var fs = File.Create(filePath);
        await content.CopyToAsync(fs, cancellationToken);

        return filePath;
    }

    public Task DeleteAsync(string filePath, CancellationToken cancellationToken = default)
    {
        if (File.Exists(filePath))
            File.Delete(filePath);
        return Task.CompletedTask;
    }

    public Task<(Stream Content, string MimeType)> ReadAsync(string filePath, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException("File not found.", filePath);

        var mimeType = _contentTypeProvider.TryGetContentType(filePath, out var ct)
            ? ct
            : "application/octet-stream";

        Stream stream = File.OpenRead(filePath);
        return Task.FromResult((stream, mimeType));
    }
}
