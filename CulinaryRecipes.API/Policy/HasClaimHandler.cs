﻿using Microsoft.AspNetCore.Authorization;

namespace CulinaryRecipes.API.Policy
{
    public class HasClaimHandler : AuthorizationHandler<HasClaimRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, HasClaimRequirement requirement)
        {
            if (!context.User.HasClaim(c => c.Type == "Permission"))
                return Task.CompletedTask;

            var userClaims = context.User.Claims.Where(c => c.Type == "Permission").ToList();
            if (userClaims.Any(c => c.Value == requirement.UserClaims))
            {
                context.Succeed(requirement);
            }
            return Task.CompletedTask;
        }
    }
}
