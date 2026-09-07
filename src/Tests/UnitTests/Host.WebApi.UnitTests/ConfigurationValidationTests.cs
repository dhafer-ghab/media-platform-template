using Host.WebApi.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Users.Infrastracture;
using Users.Infrastracture.Security;

namespace Host.WebApi.UnitTests;

public sealed class ConfigurationValidationTests
{
    [Fact]
    public void ValidConfiguration_BindsAllRequiredOptions()
    {
        using var provider = BuildProvider(ValidSettings());

        var database = provider.ValidateHostConfiguration();
        provider.ValidateUsersConfiguration();

        Assert.True(database.ApplyMigrations);
        Assert.Equal(2, provider.GetRequiredService<IOptions<ReverseProxyOptions>>().Value.ForwardLimit);
        Assert.Equal("issuer", provider.GetRequiredService<IOptions<JwtOptions>>().Value.Issuer);
    }

    [Theory]
    [InlineData("Cors:AllowedOrigins:0", "https://example.com/path")]
    [InlineData("Cors:AllowedOrigins:0", "https://example.com/")]
    [InlineData("ReverseProxy:ForwardLimit", "0")]
    [InlineData("ReverseProxy:KnownProxies:0", "not-an-ip")]
    [InlineData("Jwt:SecretKey", "too-short")]
    [InlineData("Jwt:ExpirationMinutes", "0")]
    public void InvalidConfiguration_IsRejected(string key, string value)
    {
        var settings = ValidSettings();
        settings[key] = value;
        using var provider = BuildProvider(settings);

        Assert.Throws<OptionsValidationException>(() =>
        {
            provider.ValidateHostConfiguration();
            provider.ValidateUsersConfiguration();
        });
    }

    [Fact]
    public void MissingMigrationPolicy_IsRejected()
    {
        var settings = ValidSettings();
        settings.Remove("Database:ApplyMigrations");
        using var provider = BuildProvider(settings);

        Assert.Throws<OptionsValidationException>(() => provider.ValidateHostConfiguration());
    }

    [Fact]
    public void MissingConnectionString_IsRejected()
    {
        var settings = ValidSettings();
        settings.Remove("ConnectionStrings:PostgreConnectionString");

        Assert.Throws<InvalidOperationException>(() => BuildProvider(settings));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("*")]
    [InlineData("api.example.test;*")]
    [InlineData("0.0.0.0")]
    [InlineData("[::]")]
    public void MissingOrWildcardAllowedHosts_IsRejected(string? allowedHosts)
    {
        var settings = ValidSettings();
        settings["AllowedHosts"] = allowedHosts;

        Assert.Throws<InvalidOperationException>(() => BuildProvider(settings));
    }

    private static ServiceProvider BuildProvider(Dictionary<string, string?> settings)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddHostConfiguration(configuration);
        services.AddUsersInfrastructure(configuration);
        return services.BuildServiceProvider();
    }

    private static Dictionary<string, string?> ValidSettings() => new()
    {
        ["ConnectionStrings:PostgreConnectionString"] =
            "Host=localhost;Database=test;Username=test;Password=test",
        ["AllowedHosts"] = "api.example.test",
        ["Cors:AllowedOrigins:0"] = "https://frontend.example.test",
        ["ReverseProxy:ForwardLimit"] = "2",
        ["ReverseProxy:KnownProxies:0"] = "127.0.0.1",
        ["Database:ApplyMigrations"] = "true",
        ["Jwt:Issuer"] = "issuer",
        ["Jwt:Audience"] = "audience",
        ["Jwt:SecretKey"] = "a-secure-test-key-with-at-least-32-characters",
        ["Jwt:ExpirationMinutes"] = "60"
    };
}
