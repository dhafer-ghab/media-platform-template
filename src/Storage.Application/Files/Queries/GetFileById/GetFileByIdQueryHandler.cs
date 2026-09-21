using SharedKernal.Messaging;
using SharedKernal.Results;
using Storage.Application.Abstractions;

namespace Storage.Application.Files.Queries.GetFileById;

internal sealed class GetFileByIdQueryHandler(IFileRepository fileRepository)
    : IQueryHandler<GetFileByIdQuery, Result<FileDto>>
{
    public async Task<Result<FileDto>> Handle(GetFileByIdQuery request, CancellationToken cancellationToken)
    {
        var mediaAsset = await fileRepository.GetByIdAsync(request.Id, cancellationToken);

        if (mediaAsset is null)
            return Error.NotFound(ErrorCodes.NotFound, $"File with id {request.Id} not found.");

        return mediaAsset.ToDto();
    }
}
