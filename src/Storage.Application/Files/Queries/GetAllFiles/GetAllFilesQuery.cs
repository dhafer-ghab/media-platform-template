using SharedKernal.Messaging;
using SharedKernal.Results;

namespace Storage.Application.Files.Queries.GetAllFiles;

internal sealed record GetAllFilesQuery : IQuery<Result<IReadOnlyList<FileDto>>>;
