using SharedKernal.Messaging;
using SharedKernal.Results;
using Storage.Application.Abstractions;

namespace Storage.Application.Files.Commands.DeleteFile;

internal sealed class DeleteFileCommandHandler(
    IFileRepository fileRepository,
    IFileStorageService fileStorageService)
    : ICommandHandler<DeleteFileCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(DeleteFileCommand request, CancellationToken cancellationToken)
    {
        var mediaAsset = await fileRepository.GetByIdAsync(request.Id, cancellationToken);

        if (mediaAsset is null)
            return Error.NotFound(ErrorCodes.NotFound, $"File with id {request.Id} not found.");

        try
        {
            await fileStorageService.DeleteAsync(mediaAsset.StorageKey, cancellationToken);
        }
        catch (Exception)
        {
            return Error.Failure("File.StorageDeleteFailed", "Failed to delete the file from storage.");
        }

        fileRepository.Remove(mediaAsset);
        await fileRepository.SaveChangesAsync(cancellationToken);

        return true;
    }
}
