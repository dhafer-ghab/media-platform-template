namespace Storage.Presentation.Files;

public sealed record FileResponse(
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
