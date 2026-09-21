using Microsoft.EntityFrameworkCore;
using Storage.Application.Abstractions;
using Storage.Domain;

namespace Storage.Infrastracture.Persistence;

internal class FileRepository(StorageDbContext context) : IFileRepository
{
    public async Task<MediaAsset?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.MediaAssets.FindAsync([id], cancellationToken);
    }

    public async Task<IReadOnlyList<MediaAsset>> ListAllAsync(CancellationToken cancellationToken = default)
    {
        return await context.MediaAssets
            .OrderByDescending(m => m.CreatedDate)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(MediaAsset mediaAsset, CancellationToken cancellationToken = default)
    {
        await context.MediaAssets.AddAsync(mediaAsset, cancellationToken);
    }

    public void Remove(MediaAsset mediaAsset)
    {
        context.MediaAssets.Remove(mediaAsset);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await context.SaveChangesAsync(cancellationToken);
    }
}
