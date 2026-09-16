using Moq;
using Storage.Application.Abstractions;
using Storage.Application.Files.Commands.UploadFile;
using Storage.Domain;

namespace Storage.Application.UnitTests;

public sealed class UploadFileCommandHandlerTests
{
    private readonly Mock<IFileStorageService> _storageServiceMock;
    private readonly Mock<IFileRepository> _repositoryMock;
    private readonly UploadFileCommandHandler _handler;

    public UploadFileCommandHandlerTests()
    {
        _storageServiceMock = new Mock<IFileStorageService>();
        _repositoryMock = new Mock<IFileRepository>();
        _handler = new UploadFileCommandHandler(_storageServiceMock.Object, _repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidFile_UploadsAndPersistsMediaAsset_ReturnsId()
    {
        // Arrange
        var fileId = Guid.NewGuid();
        var uploadResult = new FileUploadResult("S3-Compatible", "bucket", "uploads/2025/01/01/key.txt", "http://url");
        MediaAsset? capturedAsset = null;

        _storageServiceMock
            .Setup(s => s.UploadAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(uploadResult);

        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<MediaAsset>(), It.IsAny<CancellationToken>()))
            .Callback<MediaAsset, CancellationToken>((asset, _) => capturedAsset = asset)
            .Returns(Task.CompletedTask);

        _repositoryMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        await using var stream = new MemoryStream([0x01, 0x02, 0x03]);
        var command = new UploadFileCommand(stream, "document.txt", "text/plain", stream.Length);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value);
        Assert.NotNull(capturedAsset);
        Assert.Equal("document.txt", capturedAsset.OriginalFileName);
        Assert.Equal("text/plain", capturedAsset.ContentType);
        Assert.Equal(stream.Length, capturedAsset.FileSize);
        Assert.Equal(uploadResult.StorageProvider, capturedAsset.StorageProvider);
        Assert.Equal(uploadResult.BucketName, capturedAsset.BucketName);
        Assert.Equal(uploadResult.StorageKey, capturedAsset.StorageKey);
        Assert.Equal(uploadResult.Url, capturedAsset.Url);

        _storageServiceMock.Verify(s => s.UploadAsync(stream, "document.txt", "text/plain", It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(r => r.AddAsync(capturedAsset, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithEmptyFile_ReturnsValidationError()
    {
        // Arrange
        await using var stream = new MemoryStream();
        var command = new UploadFileCommand(stream, "empty.txt", "text/plain", 0);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("File.Empty", result.Error.Code);

        _storageServiceMock.Verify(s => s.UploadAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<MediaAsset>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithTooLargeFile_ReturnsValidationError()
    {
        // Arrange
        await using var stream = new MemoryStream();
        var command = new UploadFileCommand(stream, "big.bin", "application/octet-stream", 100 * 1024 * 1024 + 1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("File.TooLarge", result.Error.Code);

        _storageServiceMock.Verify(s => s.UploadAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<MediaAsset>(), It.IsAny<CancellationToken>()), Times.Never);
    }

}
