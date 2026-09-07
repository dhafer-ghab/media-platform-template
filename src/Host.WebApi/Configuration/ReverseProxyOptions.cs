namespace Host.WebApi.Configuration;

internal sealed class ReverseProxyOptions
{
    public const string SectionName = "ReverseProxy";

    public int ForwardLimit { get; init; }
    public string[] KnownProxies { get; init; } = [];
}
