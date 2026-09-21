namespace Storage.Application.Abstractions;

internal interface IFileImportQueue
{
    ValueTask EnqueueAsync(string sourceUrl, CancellationToken cancellationToken = default);

    IAsyncEnumerable<string> ReadAllAsync(CancellationToken cancellationToken);
}
