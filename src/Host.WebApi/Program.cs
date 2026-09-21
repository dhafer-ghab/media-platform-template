using Host.WebApi;
using Host.WebApi.ArtworkViews;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Storage.Infrastracture;
using Storage.Presentation;
using Scalar.AspNetCore;
using System.Net;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;


var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.RegisterModules();

var connectionString = builder.Configuration.GetConnectionString("PostgreConnectionString")
    ?? throw new InvalidOperationException("Connection string 'PostgreConnectionString' is not configured.");
builder.Services.AddDbContextPool<ArtworkViewsDbContext>(options =>
    options.UseNpgsql(connectionString, npgsql =>
        npgsql.MigrationsHistoryTable("__ArtworkViewsMigrations", "analytics")));
builder.Services.AddSingleton<ArtworkSlugCatalog>();
builder.Services.AddScoped<ArtworkViewStore>();
builder.Services.AddHealthChecks().AddCheck<ArtworkViewsHealthCheck>("artwork-views-db");

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.ConfigureHttpJsonOptions(o => o.SerializerOptions.Converters.Add(new JsonStringEnumConverter(null, false)));


const string PublicFrontendCors = "public-frontend";
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
if (!builder.Environment.IsDevelopment() && allowedOrigins.Length == 0)
    throw new InvalidOperationException("Cors:AllowedOrigins must contain at least one production frontend origin.");

builder.Services.AddCors(options =>
{
    options.AddPolicy(PublicFrontendCors, policy =>
    {
        policy.WithOrigins(allowedOrigins.Length > 0 ? allowedOrigins : ["http://localhost:4321"])
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    var forwardLimit = builder.Configuration.GetValue<int?>("ReverseProxy:ForwardLimit") ?? 1;
    if (forwardLimit < 1)
        throw new InvalidOperationException("ReverseProxy:ForwardLimit must be at least 1.");
    options.ForwardLimit = forwardLimit;
    foreach (var proxy in builder.Configuration.GetSection("ReverseProxy:KnownProxies").Get<string[]>() ?? [])
    {
        if (!IPAddress.TryParse(proxy, out var address))
            throw new InvalidOperationException($"ReverseProxy:KnownProxies contains invalid address '{proxy}'.");
        options.KnownProxies.Add(address);
    }
});

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("artwork-view-recording", context => CreateLimiter(GetClientAddress(context), 30));
    options.AddPolicy("artwork-view-ranking", context => CreateLimiter(GetClientAddress(context), 120));
});

var app = builder.Build();

app.MapDefaultEndpoints();
app.UseForwardedHeaders();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.OpenApiRoutePattern = "/openapi/v1.json";
    });
}

await app.ApplyMigrations();

app.UseCors(PublicFrontendCors);
app.UseHttpsRedirection();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

// map controllers from all registered application parts (modules)
app.MapControllers();
app.MapArtworkViewEndpoints();

app.Run();

static RateLimitPartition<string> CreateLimiter(string key, int permitLimit) =>
    RateLimitPartition.GetFixedWindowLimiter(key, _ => new FixedWindowRateLimiterOptions
    {
        PermitLimit = permitLimit,
        Window = TimeSpan.FromMinutes(1),
        QueueLimit = 0
    });

static string GetClientAddress(HttpContext context)
{
    var address = context.Connection.RemoteIpAddress;
    if (address?.IsIPv4MappedToIPv6 == true) address = address.MapToIPv4();
    return address?.ToString() ?? "unknown";
}

public partial class Program;
