using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Storage.Application.Abstractions;
using Storage.Domain;

namespace Storage.Infrastracture.BackgroundServices;

internal sealed class FileImportBackgroundService(
    IFileImportQueue importQueue,
    IServiceScopeFactory scopeFactory,
    ILogger<FileImportBackgroundService> logger) : BackgroundService
{
    private static readonly HttpClient HttpClient = new();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var sourceUrl in importQueue.ReadAllAsync(stoppingToken))
        {
            try
            {
                await ImportAsync(sourceUrl, stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to import file from {SourceUrl}", sourceUrl);
            }
        }
    }

    private async Task ImportAsync(string sourceUrl, CancellationToken cancellationToken)
    {
        using var response = await HttpClient.GetAsync(
            sourceUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();

        var fileName = GetFileName(response, sourceUrl);
        var contentType = response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";
        var fileSize = response.Content.Headers.ContentLength ?? 0;

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);

        using var scope = scopeFactory.CreateScope();
        var fileStorageService = scope.ServiceProvider.GetRequiredService<IFileStorageService>();
        var fileRepository = scope.ServiceProvider.GetRequiredService<IFileRepository>();

        var uploadResult = await fileStorageService.UploadMultiPart(
            stream, fileName, contentType, cancellationToken);

        var mediaAsset = MediaAsset.Create(
            fileName,
            contentType,
            fileSize,
            uploadResult.StorageProvider,
            uploadResult.BucketName,
            uploadResult.StorageKey,
            uploadResult.Url);

        await fileRepository.AddAsync(mediaAsset, cancellationToken);
        await fileRepository.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Imported file {FileName} from {SourceUrl}", fileName, sourceUrl);
    }

    private static string GetFileName(HttpResponseMessage response, string sourceUrl)
    {
        var fromHeader = response.Content.Headers.ContentDisposition?.FileName?.Trim('"');
        if (!string.IsNullOrWhiteSpace(fromHeader))
            return fromHeader;

        var lastSegment = new Uri(sourceUrl).AbsolutePath
            .Split('/', StringSplitOptions.RemoveEmptyEntries)
            .LastOrDefault();

        return string.IsNullOrWhiteSpace(lastSegment) ? $"{Guid.NewGuid():N}" : lastSegment;
    }
}
