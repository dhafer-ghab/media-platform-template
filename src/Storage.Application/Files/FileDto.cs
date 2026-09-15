namespace Storage.Application.Files;

internal sealed record FileDto(
    Guid Id,
    string FileName,
    string OriginalFileName,
    string ContentType,
    long FileSize,
    string StorageProvider,
    string BucketName,
    string StorageKey,
    string Url,
    DateTime CreatedDate);
