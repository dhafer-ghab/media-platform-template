using System.Net;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Options;

namespace Host.WebApi.Configuration;

internal static class ConfigurationExtensions
{
    public static IServiceCollection AddHostConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var allowedHosts = configuration["AllowedHosts"];
        if (string.IsNullOrWhiteSpace(allowedHosts) ||
            allowedHosts.Split(';', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                .Any(IsWildcardHost))
            throw new InvalidOperationException(
                "AllowedHosts must explicitly list the hosts accepted by the API.");

        var corsSection = configuration.GetSection(CorsOptions.SectionName);
        var configuredOrigins = corsSection.Get<CorsOptions>()?.AllowedOrigins ?? [];

        services.AddOptions<CorsOptions>()
            .Bind(corsSection)
            .Validate(
                options => options.AllowedOrigins.Length > 0,
                $"{CorsOptions.SectionName}:AllowedOrigins must contain at least one origin.")
            .Validate(
                options => options.AllowedOrigins.All(IsOrigin),
                $"{CorsOptions.SectionName}:AllowedOrigins must contain absolute HTTP or HTTPS origins without paths.")
            .ValidateOnStart();

        services.AddOptions<ReverseProxyOptions>()
            .Bind(configuration.GetSection(ReverseProxyOptions.SectionName))
            .Validate(
                options => options.ForwardLimit > 0,
                $"{ReverseProxyOptions.SectionName}:ForwardLimit must be greater than zero.")
            .Validate(
                options => options.KnownProxies.All(value => IPAddress.TryParse(value, out _)),
                $"{ReverseProxyOptions.SectionName}:KnownProxies must contain valid IP addresses.")
            .ValidateOnStart();

        services.AddOptions<DatabaseOptions>()
            .Bind(configuration.GetSection(DatabaseOptions.SectionName))
            .Validate(
                options => options.ApplyMigrations.HasValue,
                $"{DatabaseOptions.SectionName}:ApplyMigrations must be explicitly configured.")
            .ValidateOnStart();

        services.AddCors(cors => cors.AddPolicy(
            CorsOptions.PublicFrontendPolicy,
            policy => policy
                .WithOrigins(configuredOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod()));

        services.AddOptions<ForwardedHeadersOptions>()
            .Configure<IOptions<ReverseProxyOptions>>((forwarded, configured) =>
            {
                forwarded.ForwardedHeaders =
                    ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
                forwarded.ForwardLimit = configured.Value.ForwardLimit;

                foreach (var proxy in configured.Value.KnownProxies)
                    forwarded.KnownProxies.Add(IPAddress.Parse(proxy));
            });

        return services;
    }

    public static DatabaseOptions ValidateHostConfiguration(this IServiceProvider services)
    {
        _ = services.GetRequiredService<IOptions<CorsOptions>>().Value;
        _ = services.GetRequiredService<IOptions<ReverseProxyOptions>>().Value;
        return services.GetRequiredService<IOptions<DatabaseOptions>>().Value;
    }

    private static bool IsOrigin(string value) =>
        !value.EndsWith('/') &&
        Uri.TryCreate(value, UriKind.Absolute, out var uri) &&
        (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps) &&
        uri.AbsolutePath == "/" &&
        string.IsNullOrEmpty(uri.Query) &&
        string.IsNullOrEmpty(uri.Fragment) &&
        string.IsNullOrEmpty(uri.UserInfo);

    private static bool IsWildcardHost(string value) =>
        value.Contains('*') || value is "0.0.0.0" or "[::]";
}
