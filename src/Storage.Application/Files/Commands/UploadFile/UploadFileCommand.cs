using SharedKernal.Messaging;
using SharedKernal.Results;

namespace Storage.Application.Files.Commands.UploadFile;

internal sealed record UploadFileCommand(
    Stream FileStream,
    string OriginalFileName,
    string ContentType,
    long FileSize) : ICommand<Result<Guid>>;
