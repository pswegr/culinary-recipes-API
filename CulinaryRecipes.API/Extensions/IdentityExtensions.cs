

using AspNetCore.Identity.Mongo;
using AspNetCore.Identity.MongoDbCore.Infrastructure;
using CulinaryRecipes.API.Data;
using CulinaryRecipes.API.Data.Interfaces;
using CulinaryRecipes.API.Models.Identity;
using CulinaryRecipes.API.Policy;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace CulinaryRecipes.API.Extensions
{
    public static class IdentityExtensions
    {
        public static IServiceCollection AddApplicationIdentityServices(this IServiceCollection services, IConfiguration config)
        {
            var dbIdentitySettings = config.GetSection("IdentityDatabase").Get<IdentityDatabaseSettings>();
            var jwtSettings = config.GetSection("Jwt").Get<JwtSettings>();

            services.AddIdentityMongoDbProvider<ApplicationUser, ApplicationRole>(identityOptions =>
            {
                identityOptions.Password.RequireDigit = true;
                identityOptions.Password.RequireLowercase = true;
                identityOptions.Password.RequireUppercase = true;
                identityOptions.Password.RequireNonAlphanumeric = true;
                identityOptions.Password.RequiredLength = 6;
                identityOptions.User.RequireUniqueEmail = true;
                identityOptions.Tokens.EmailConfirmationTokenProvider = TokenOptions.DefaultProvider;
                identityOptions.Tokens.PasswordResetTokenProvider = TokenOptions.DefaultProvider;
                identityOptions.SignIn.RequireConfirmedAccount = true;
            },
            mongoIdentityOptions =>
            {
                mongoIdentityOptions.ConnectionString = dbIdentitySettings.ConnectionString;
                mongoIdentityOptions.RolesCollection = dbIdentitySettings.RolesCollectionName;
                mongoIdentityOptions.UsersCollection = dbIdentitySettings.UsersCollectionName;
            }).AddUserManager<UserManager<ApplicationUser>>()
              .AddSignInManager<SignInManager<ApplicationUser>>()
              .AddRoleManager<RoleManager<ApplicationRole>>()
              .AddDefaultTokenProviders();
            // Configure JWT Authentication

            services.AddSingleton<IAuthorizationPolicyProvider, AuthorizationPolicyProvider>();
            services.AddSingleton<IAuthorizationHandler, HasClaimHandler>();
            services.AddScoped<IDataSeeder, DataSeeder>();



            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = true;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key))
                };
            });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("Admin", policy => policy.RequireClaim("Role", "Admin"));
            });

            return services;
        }
    }
}
