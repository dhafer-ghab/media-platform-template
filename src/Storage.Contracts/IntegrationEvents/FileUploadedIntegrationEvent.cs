namespace Storage.Contracts.IntegrationEvents;

public sealed record FileUploadedIntegrationEvent(
    Guid FileId,
    string OriginalFileName,
    string ContentType,
    long FileSize,
    string Url,
    DateTime UploadedAt);
