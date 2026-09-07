var builder = DistributedApplication.CreateBuilder(args);

var password = builder.AddParameter("password", secret: true);
var jwtSecret = builder.AddParameter("jwt-secret", secret: true);
var jwtIssuer = builder.AddParameter("jwt-issuer");
var jwtAudience = builder.AddParameter("jwt-audience");
var jwtExpirationMinutes = builder.AddParameter("jwt-expiration-minutes");

var postgres = builder.AddPostgres("postgres", password: password)
    .WithPgAdmin()
    .WithDataVolume();

var database = postgres.AddDatabase("PostgreConnectionString");

var api = builder.AddProject<Projects.Host_WebApi>("host-webapi")
    .WithHttpEndpoint(name: "http")
    .WithReference(database)
    .WithEnvironment("Jwt__Issuer", jwtIssuer)
    .WithEnvironment("Jwt__Audience", jwtAudience)
    .WithEnvironment("Jwt__SecretKey", jwtSecret)
    .WithEnvironment("Jwt__ExpirationMinutes", jwtExpirationMinutes)
    .WithEnvironment("Database__ApplyMigrations", "true")
    .WaitFor(database);

api.WithEnvironment("AllowedHosts", api.GetEndpoint("http").Property(EndpointProperty.Host));

var frontend = builder.AddNpmApp("frontend", "../../AstroFrontend", "start")
    .WithHttpEndpoint(env: "PORT", name: "http")
    .WithExternalHttpEndpoints()
    .WithReference(api)
    .WithEnvironment("PUBLIC_API_BASE_URL", api.GetEndpoint("http"));

frontend.WithEnvironment("PUBLIC_SITE_URL", frontend.GetEndpoint("http"));

api.WithEnvironment("Cors__AllowedOrigins__0", frontend.GetEndpoint("http"))
    .WithEnvironment("ReverseProxy__ForwardLimit", "1");

builder.Build().Run();
