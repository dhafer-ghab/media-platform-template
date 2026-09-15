using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel.Entities.Enums;
using Users.Presentation.Authorization.Handlers;
using Users.Presentation.Authorization.Requirements;

namespace Users.Presentation.Authorization;

public static class UsersAuthorizationExtensions
{
    public static IServiceCollection AddUsersAuthorization(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<IAuthorizationHandler, SameUserOrAdminAuthorizationHandler>();

        services.AddAuthorizationBuilder()
            .AddPolicy(UsersPolicies.RequireAdmin, policy =>
                policy.RequireRole(nameof(Role.Admin)))
            .AddPolicy(UsersPolicies.RequirePremiumUser, policy =>
                policy.RequireRole(nameof(Role.PremiumUser), nameof(Role.Admin)))
            .AddPolicy(UsersPolicies.CanManageUsers, policy =>
                policy.Requirements.Add(new SameUserOrAdminRequirement()));

        return services;
    }
}
