using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SharedKernal.Results;
using Storage.Application.Files.Commands.UploadFile;
using Storage.Presentation.Files;

namespace Storage.Presentation.UnitTests;

public sealed class FilesControllerTests
{
    private readonly Mock<ISender> _senderMock;
    private readonly FilesController _controller;

    public FilesControllerTests()
    {
        _senderMock = new Mock<ISender>();
        _controller = new FilesController(_senderMock.Object);
    }

    [Fact]
    public async Task Upload_WithValidFile_ForwardsFileDataToCommand_AndReturnsCreatedWithLocation()
    {
        // Arrange
        var fileId = Guid.NewGuid();
        var (file, stream) = CreateFormFile("document.txt", "text/plain", [0x01, 0x02, 0x03]);
        UploadFileCommand? sentCommand = null;

        _senderMock
            .Setup(s => s.Send(It.IsAny<UploadFileCommand>(), It.IsAny<CancellationToken>()))
            .Callback<IRequest<Result<Guid>>, CancellationToken>((request, _) =>
                sentCommand = (UploadFileCommand)request)
            .ReturnsAsync(Result.Success(fileId));

        // Act
        var result = await _controller.Upload(file.Object, CancellationToken.None);

        // Assert
        Assert.NotNull(sentCommand);
        Assert.Equal("document.txt", sentCommand.OriginalFileName);
        Assert.Equal("text/plain", sentCommand.ContentType);
        Assert.Equal(stream.Length, sentCommand.FileSize);
        Assert.Same(stream, sentCommand.FileStream);

        var createdResult = Assert.IsType<CreatedResult>(result);
        Assert.Equal($"/api/files/{fileId}", createdResult.Location);

        var value = createdResult.Value!;
        var idProperty = value.GetType().GetProperty("id");
        Assert.NotNull(idProperty);
        Assert.Equal(fileId, idProperty.GetValue(value));
    }

    [Fact]
    public async Task Upload_WithEmptyFile_ReturnsBadRequest()
    {
        // Arrange
        var (file, _) = CreateFormFile("empty.txt", "text/plain", []);
        var error = Error.Validation("File.Empty", "File cannot be empty.");

        _senderMock
            .Setup(s => s.Send(It.IsAny<UploadFileCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<Guid>(error));

        // Act
        var result = await _controller.Upload(file.Object, CancellationToken.None);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(400, badRequestResult.StatusCode);
    }

    [Fact]
    public async Task Upload_WithTooLargeFile_ReturnsBadRequest()
    {
        // Arrange — the file reports a size over the 100 MB limit without allocating real bytes
        var (file, _) = CreateFormFile(
            "big.bin", "application/octet-stream", [0x01],
            lengthOverride: 100L * 1024 * 1024 + 1);
        var error = Error.Validation("File.TooLarge", "File size exceeds 100 MB limit.");

        _senderMock
            .Setup(s => s.Send(It.IsAny<UploadFileCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<Guid>(error));

        // Act
        var result = await _controller.Upload(file.Object, CancellationToken.None);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(400, badRequestResult.StatusCode);
    }

    private static (Mock<IFormFile> File, MemoryStream Stream) CreateFormFile(
        string fileName, string contentType, byte[] content, long? lengthOverride = null)
    {
        var stream = new MemoryStream(content);
        var fileMock = new Mock<IFormFile>();

        fileMock.Setup(f => f.OpenReadStream()).Returns(stream);
        fileMock.Setup(f => f.FileName).Returns(fileName);
        fileMock.Setup(f => f.ContentType).Returns(contentType);
        fileMock.Setup(f => f.Length).Returns(lengthOverride ?? stream.Length);

        return (fileMock, stream);
    }
}
