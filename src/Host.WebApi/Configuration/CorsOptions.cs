namespace Host.WebApi.Configuration;

internal sealed class CorsOptions
{
    public const string SectionName = "Cors";
    public const string PublicFrontendPolicy = "PublicFrontend";

    public string[] AllowedOrigins { get; init; } = [];
}
