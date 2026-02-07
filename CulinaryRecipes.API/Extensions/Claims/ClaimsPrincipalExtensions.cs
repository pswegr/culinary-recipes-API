using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace CulinaryRecipes.API.Extensions.Claims
{
    public static class ClaimsPrincipalExtensions
    {
        public static string? GetUserId(this ClaimsPrincipal principal)
        {
            return principal.FindFirstValue(ClaimTypes.NameIdentifier) ??
                   principal.FindFirstValue(JwtRegisteredClaimNames.Sub);
        }
    }
}
