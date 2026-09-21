using SharedKernal.Messaging;
using SharedKernal.Results;
using Storage.Application.Abstractions;
using Storage.Domain;

namespace Storage.Application.Files.Commands.UploadFile;

internal sealed class UploadFileCommandHandler(
    IFileStorageService fileStorageService,
    IFileRepository fileRepository)
    : ICommandHandler<UploadFileCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(UploadFileCommand request, CancellationToken cancellationToken)
    {
        if (request.FileSize <= 0)
            return Result.Failure<Guid>(Error.Validation("File.Empty", "File cannot be empty."));

        if (request.FileSize > 100 * 1024 * 1024)
            return Error.Validation("File.TooLarge", "File size exceeds 100 MB limit."); // TODO: change later

        var uploadResult = await fileStorageService.UploadAsync(
            request.FileStream,
            request.OriginalFileName,
            request.ContentType,
            cancellationToken);

        var mediaAsset = MediaAsset.Create(
            request.OriginalFileName,
            request.ContentType,
            request.FileSize,
            uploadResult.StorageProvider,
            uploadResult.BucketName,
            uploadResult.StorageKey,
            uploadResult.Url);

        await fileRepository.AddAsync(mediaAsset, cancellationToken);
        await fileRepository.SaveChangesAsync(cancellationToken);

        return mediaAsset.Id;
    }
}
