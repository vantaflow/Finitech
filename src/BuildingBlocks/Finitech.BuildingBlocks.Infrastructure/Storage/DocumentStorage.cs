using System.Security.Cryptography;
using Microsoft.Extensions.Logging;

namespace Finitech.BuildingBlocks.Infrastructure.Storage;

public interface IDocumentStorage
{
    Task<string> UploadAsync(Stream content, string fileName, string contentType, CancellationToken cancellationToken = default);
    Task<Stream?> DownloadAsync(string documentId, CancellationToken cancellationToken = default);
    Task<string> GeneratePresignedUrlAsync(string documentId, TimeSpan expiry, CancellationToken cancellationToken = default);
    Task DeleteAsync(string documentId, CancellationToken cancellationToken = default);
}

public class LocalDocumentStorage : IDocumentStorage
{
    private readonly string _storagePath;
    private readonly ILogger<LocalDocumentStorage> _logger;

    public LocalDocumentStorage(ILogger<LocalDocumentStorage> logger, string? storagePath = null)
    {
        _logger = logger;
        _storagePath = storagePath ?? Path.Combine(AppContext.BaseDirectory, "uploads");
        Directory.CreateDirectory(_storagePath);
    }

    public async Task<string> UploadAsync(Stream content, string fileName, string contentType, CancellationToken cancellationToken = default)
    {
        var documentId = $"{Guid.NewGuid():N}{Path.GetExtension(fileName)}";
        var filePath = Path.Combine(_storagePath, documentId);

        await using var fileStream = File.Create(filePath);
        await content.CopyToAsync(fileStream, cancellationToken);

        _logger.LogInformation("[MOCK STORAGE] Uploaded {FileName} as {DocumentId}", fileName, documentId);
        return documentId;
    }

    public Task<Stream?> DownloadAsync(string documentId, CancellationToken cancellationToken = default)
    {
        var filePath = Path.Combine(_storagePath, documentId);
        if (!File.Exists(filePath))
        {
            return Task.FromResult<Stream?>(null);
        }

        return Task.FromResult<Stream?>(File.OpenRead(filePath));
    }

    public Task<string> GeneratePresignedUrlAsync(string documentId, TimeSpan expiry, CancellationToken cancellationToken = default)
    {
        // In production: generate S3 presigned URL
        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        return Task.FromResult($"/api/documents/download/{documentId}?token={token}&expires={DateTime.UtcNow.Add(expiry):O}");
    }

    public Task DeleteAsync(string documentId, CancellationToken cancellationToken = default)
    {
        var filePath = Path.Combine(_storagePath, documentId);
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            _logger.LogInformation("[MOCK STORAGE] Deleted {DocumentId}", documentId);
        }
        return Task.CompletedTask;
    }
}
