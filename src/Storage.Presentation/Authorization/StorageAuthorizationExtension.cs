using Microsoft.Extensions.DependencyInjection;
using SharedKernel.Entities.Enums;

namespace Storage.Presentation.Authorization
{
    public static class StorageAuthorizationExtension
    {
        public static IServiceCollection AddStorageAuthorization(this IServiceCollection services)
        {
            services.AddAuthorizationBuilder()
               .AddPolicy(StoragePolicies.RequireAdmin, policy =>
                   policy.RequireRole(nameof(Role.Admin)));

            return services;
        }
    }
}
