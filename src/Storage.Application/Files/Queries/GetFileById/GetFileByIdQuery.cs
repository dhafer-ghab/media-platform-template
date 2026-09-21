using SharedKernal.Messaging;
using SharedKernal.Results;

namespace Storage.Application.Files.Queries.GetFileById;

internal sealed record GetFileByIdQuery(Guid Id) : IQuery<Result<FileDto>>;
