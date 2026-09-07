using Host.WebApi;
using Host.WebApi.Configuration;
using Scalar.AspNetCore;
using System.Text.Json.Serialization;
using Users.Infrastracture;


var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.RegisterModules();
builder.Services.AddHostConfiguration(builder.Configuration);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.ConfigureHttpJsonOptions(o => o.SerializerOptions.Converters.Add(new JsonStringEnumConverter(null, false)));


var app = builder.Build();
var databaseOptions = app.Services.ValidateHostConfiguration();
app.Services.ValidateUsersConfiguration();

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

if (databaseOptions.ApplyMigrations == true)
    await app.ApplyMigrations();

app.UseCors(CorsOptions.PublicFrontendPolicy);
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// map controllers from all registered application parts (modules)
app.MapControllers();

app.Run();
