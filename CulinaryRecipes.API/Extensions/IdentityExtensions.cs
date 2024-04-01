using CulinaryRecipes.API.Helpers;
using CulinaryRecipes.API.Services.Interfaces;
using CulinaryRecipes.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace CulinaryRecipes.API.Extensions
{
    public static class IdentityExtensions
    {
        public static IServiceCollection AddApplicationIdentityServices(this IServiceCollection services, IConfiguration config)
        {
            // Configure JWT Authentication
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"])),
                        ValidateIssuer = false, // Set to true and specify Issuer if needed
                        ValidateAudience = false, // Set to true and specify Audience if needed
                    };
                });

            services.AddAuthorization();

            return services;
        }
    }
}
