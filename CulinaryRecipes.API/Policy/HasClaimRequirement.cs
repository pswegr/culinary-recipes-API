using Microsoft.AspNetCore.Authorization;

namespace CulinaryRecipes.API.Policy
{
    public class HasClaimRequirement : IAuthorizationRequirement
    {
        public string UserClaims { get; set; }
        public HasClaimRequirement(string userClaims)
        {
            UserClaims = userClaims ?? throw new ArgumentNullException(nameof(userClaims));
        }
    }
}
