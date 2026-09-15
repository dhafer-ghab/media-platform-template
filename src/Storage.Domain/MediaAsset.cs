using SharedKernal.Entities;

namespace Storage.Domain;

public class MediaAsset : AggregateRoot
{
    public string FileName { get; private set; }
    public string OriginalFileName { get; private set; }
    public string ContentType { get; private set; }
    public long FileSize { get; private set; }
    public string StorageProvider { get; private set; }
    public string BucketName { get; private set; }
    public string StorageKey { get; private set; }
    public string Url { get; private set; }
    private MediaAsset(
        Guid id,
        string fileName,
        string originalFileName,
        string contentType,
        long fileSize,
        string storageProvider,
        string bucketName,
        string storageKey,
        string url)
        : base(id)
    {
        FileName = fileName;
        OriginalFileName = originalFileName;
        ContentType = contentType;
        FileSize = fileSize;
        StorageProvider = storageProvider;
        BucketName = bucketName;
        StorageKey = storageKey;
        Url = url;
    }

    private MediaAsset()
    {
        FileName = null!;
        OriginalFileName = null!;
        ContentType = null!;
        StorageProvider = null!;
        BucketName = null!;
        StorageKey = null!;
        Url = null!;
    }

    public static MediaAsset Create(
        string originalFileName,
        string contentType,
        long fileSize,
        string storageProvider,
        string bucketName,
        string storageKey,
        string url)
    {
        var fileName = Guid.NewGuid().ToString("N") + Path.GetExtension(originalFileName);

        return new MediaAsset(
            Guid.NewGuid(),
            fileName,
            originalFileName,
            contentType,
            fileSize,
            storageProvider,
            bucketName,
            storageKey,
            url);
    }
}
