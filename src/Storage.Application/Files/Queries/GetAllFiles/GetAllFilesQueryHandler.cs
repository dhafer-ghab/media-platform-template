using SharedKernal.Messaging;
using SharedKernal.Results;
using Storage.Application.Abstractions;

namespace Storage.Application.Files.Queries.GetAllFiles;

internal sealed class GetAllFilesQueryHandler(IFileRepository fileRepository)
    : IQueryHandler<GetAllFilesQuery, Result<IReadOnlyList<FileDto>>>
{
    public async Task<Result<IReadOnlyList<FileDto>>> Handle(
        GetAllFilesQuery request,
        CancellationToken cancellationToken)
    {
        var files = await fileRepository.ListAllAsync(cancellationToken);

        return files
            .Select(f => f.ToDto())
            .ToList();
    }
}
