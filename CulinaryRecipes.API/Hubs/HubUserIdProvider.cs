using Microsoft.AspNetCore.SignalR;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace CulinaryRecipes.API.Hubs
{
    public class HubUserIdProvider : IUserIdProvider
    {
        public string? GetUserId(HubConnectionContext connection)
        {
            return connection.User?.FindFirstValue(ClaimTypes.NameIdentifier) ??
                   connection.User?.FindFirstValue(JwtRegisteredClaimNames.Sub);
        }
    }
}
