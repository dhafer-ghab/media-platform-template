namespace Storage.Infrastracture.Storage;

internal sealed class S3Options
{
    public const string SectionName = "S3";

    public string AccessKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public string BucketName { get; set; } = string.Empty;
    public string? ServiceURL { get; set; }
    public bool ForcePathStyle { get; set; }
    public int PresignedUrlExpiryMinutes { get; set; } = 15;
}
