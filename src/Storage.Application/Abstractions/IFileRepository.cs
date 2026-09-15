using Storage.Domain;

namespace Storage.Application.Abstractions;

internal interface IFileRepository
{
    Task<MediaAsset?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MediaAsset>> ListAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(MediaAsset mediaAsset, CancellationToken cancellationToken = default);
    void Remove(MediaAsset mediaAsset);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
