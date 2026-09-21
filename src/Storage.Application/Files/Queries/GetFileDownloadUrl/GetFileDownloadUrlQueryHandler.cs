using SharedKernal.Messaging;
using SharedKernal.Results;
using Storage.Application.Abstractions;

namespace Storage.Application.Files.Queries.GetFileDownloadUrl;

internal sealed class GetFileDownloadUrlQueryHandler(
    IFileRepository fileRepository,
    IFileStorageService fileStorageService)
    : IQueryHandler<GetFileDownloadUrlQuery, Result<string>>
{
    public async Task<Result<string>> Handle(GetFileDownloadUrlQuery request, CancellationToken cancellationToken)
    {
        var mediaAsset = await fileRepository.GetByIdAsync(request.Id, cancellationToken);

        if (mediaAsset is null)
            return Error.NotFound(ErrorCodes.NotFound, $"File with id {request.Id} not found.");

        return await fileStorageService.GetPresignedUrlAsync(mediaAsset.StorageKey, cancellationToken);
    }
}
