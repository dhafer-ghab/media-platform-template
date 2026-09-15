using System.Threading.Channels;
using Storage.Application.Abstractions;

namespace Storage.Infrastracture.Storage;

internal sealed class FileImportQueue : IFileImportQueue
{
    private readonly Channel<string> _channel = Channel.CreateUnbounded<string>();

    public ValueTask EnqueueAsync(string sourceUrl, CancellationToken cancellationToken = default)
    {
        return _channel.Writer.WriteAsync(sourceUrl, cancellationToken);
    }

    public IAsyncEnumerable<string> ReadAllAsync(CancellationToken cancellationToken)
    {
        return _channel.Reader.ReadAllAsync(cancellationToken);
    }
}
