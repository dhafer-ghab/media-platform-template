using SharedKernal.Messaging;
using SharedKernal.Results;

namespace Storage.Application.Files.Commands.ImportFilesFromUrls;

internal sealed record ImportFilesFromUrlsCommand(IReadOnlyList<string> Urls) : ICommand<Result<int>>;
