using SharedKernal.Messaging;
using SharedKernal.Results;
using Storage.Application.Abstractions;

namespace Storage.Application.Files.Commands.ImportFilesFromUrls;

internal sealed class ImportFilesFromUrlsCommandHandler(IFileImportQueue importQueue)
    : ICommandHandler<ImportFilesFromUrlsCommand, Result<int>>
{
    public async Task<Result<int>> Handle(ImportFilesFromUrlsCommand request, CancellationToken cancellationToken)
    {
        if (request.Urls.Count == 0)
            return Error.Validation("FileImport.Empty", "At least one URL must be provided.");

        var validUrls = new List<string>();
        var errors = new List<Error>();

        foreach (var url in request.Urls)
        {
            if (Uri.TryCreate(url, UriKind.Absolute, out var uri) &&
                (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
            {
                validUrls.Add(url);
            }
            else
            {
                errors.Add(Error.Validation("FileImport.InvalidUrl", $"'{url}' is not a valid http(s) URL."));
            }
        }

        if (errors.Count > 0)
            return Result.Failure<int>(errors);

        foreach (var url in validUrls)
            await importQueue.EnqueueAsync(url, cancellationToken);

        return validUrls.Count;
    }
}
