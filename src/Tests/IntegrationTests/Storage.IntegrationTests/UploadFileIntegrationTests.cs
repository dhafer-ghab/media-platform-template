using Microsoft.EntityFrameworkCore;
using Storage.Application.Abstractions;
using Storage.Application.Files.Commands.UploadFile;
using Storage.Infrastracture.Persistence;

namespace Storage.IntegrationTests;

[Collection(PostgreSqlCollection.Name)]
public sealed class UploadFileIntegrationTests(PostgreSqlFixture fixture)
{
    [Fact]
    public async Task Handle_WithValidFile_PersistsMediaAssetToDatabase()
    {
        // Arrange
        await using var context = new StorageDbContext(fixture.DbContextOptions);
        var repository = new FileRepository(context);
        var handler = new UploadFileCommandHandler(new FakeFileStorageService(), repository);

        await using var stream = new MemoryStream([0x01, 0x02, 0x03]);
        var command = new UploadFileCommand(stream, "document.txt", "text/plain", stream.Length);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);

        // A fresh context forces the data to come from PostgreSQL, not EF Core's change tracker.
        await using var verificationContext = new StorageDbContext(fixture.DbContextOptions);
        var persistedAsset = await verificationContext.MediaAssets.FindAsync(result.Value);
        Assert.NotNull(persistedAsset);
        Assert.Equal("document.txt", persistedAsset.OriginalFileName);
        Assert.Equal("text/plain", persistedAsset.ContentType);
        Assert.Equal(stream.Length, persistedAsset.FileSize);
        Assert.Equal("S3-Compatible", persistedAsset.StorageProvider);
        Assert.Equal("test-bucket", persistedAsset.BucketName);
        Assert.NotNull(persistedAsset.StorageKey);
        Assert.NotNull(persistedAsset.Url);
    }

    [Fact]
    public async Task Handle_WithEmptyFile_DoesNotPersistAnything()
    {
        // Arrange
        await using var context = new StorageDbContext(fixture.DbContextOptions);
        var repository = new FileRepository(context);
        var handler = new UploadFileCommandHandler(new FakeFileStorageService(), repository);
        var countBefore = await context.MediaAssets.CountAsync();

        await using var stream = new MemoryStream();
        var command = new UploadFileCommand(stream, "empty.txt", "text/plain", 0);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("File.Empty", result.Error.Code);

        await using var verificationContext = new StorageDbContext(fixture.DbContextOptions);
        Assert.Equal(countBefore, await verificationContext.MediaAssets.CountAsync());
    }

    [Fact]
    public async Task Handle_WithTooLargeFile_DoesNotPersistAnything()
    {
        // Arrange
        await using var context = new StorageDbContext(fixture.DbContextOptions);
        var repository = new FileRepository(context);
        var handler = new UploadFileCommandHandler(new FakeFileStorageService(), repository);
        var countBefore = await context.MediaAssets.CountAsync();

        await using var stream = new MemoryStream();
        var command = new UploadFileCommand(stream, "big.bin", "application/octet-stream", 100L * 1024 * 1024 + 1);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("File.TooLarge", result.Error.Code);

        await using var verificationContext = new StorageDbContext(fixture.DbContextOptions);
        Assert.Equal(countBefore, await verificationContext.MediaAssets.CountAsync());
    }

    private sealed class FakeFileStorageService : IFileStorageService
    {
        public Task<FileUploadResult> UploadAsync(
            Stream fileStream,
            string fileName,
            string contentType,
            CancellationToken cancellationToken = default)
        {
            var storageKey = $"uploads/{Guid.NewGuid():N}{Path.GetExtension(fileName)}";
            return Task.FromResult(new FileUploadResult(
                "S3-Compatible",
                "test-bucket",
                storageKey,
                $"http://localhost/{storageKey}"));
        }

        public Task<FileUploadResult> UploadMultiPart(Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public Task DeleteAsync(string storageKey, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<string> GetPresignedUrlAsync(string storageKey, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }
    }
}
