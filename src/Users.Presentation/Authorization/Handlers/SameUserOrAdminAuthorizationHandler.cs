using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SharedKernel.Entities.Enums;
using Users.Presentation.Authorization.Requirements;

namespace Users.Presentation.Authorization.Handlers;

internal sealed class SameUserOrAdminAuthorizationHandler(IHttpContextAccessor httpContextAccessor)
    : AuthorizationHandler<SameUserOrAdminRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        SameUserOrAdminRequirement requirement)
    {
        if (context.User.IsInRole(nameof(Role.Admin)))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        var routeId = httpContextAccessor.HttpContext?.GetRouteValue("id")?.ToString();

        // "sub" is the standard JWT subject claim — update this once token issuance is implemented.
        var currentUserId = context.User.FindFirst("sub")?.Value;

        if (routeId is not null && currentUserId is not null && routeId == currentUserId)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
