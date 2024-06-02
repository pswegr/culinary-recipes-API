using CulinaryRecipes.API.Data.Interfaces;
using CulinaryRecipes.API.Models.Identity;
using Microsoft.AspNetCore.Identity;

namespace CulinaryRecipes.API.Data
{
    public class DataSeeder : IDataSeeder
    {
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;

        public DataSeeder(RoleManager<ApplicationRole> roleManager, UserManager<ApplicationUser> userManager, IConfiguration configuration)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _configuration = configuration;
        }

        public async Task SeedRolesAsync()
        {
            if (!await _roleManager.RoleExistsAsync("Admin"))
            {
                await _roleManager.CreateAsync(new ApplicationRole { Name = "Admin" });
            }

            if (!await _roleManager.RoleExistsAsync("User"))
            {
                await _roleManager.CreateAsync(new ApplicationRole { Name = "User" });
            }
        }

        public async Task SeedAdminUserAsync()
        {
            var adminDefaultUser = _configuration.GetSection("AdminUser").Get<AdminUserSettings>();
            var adminUser = await _userManager.FindByEmailAsync(adminDefaultUser.Email);
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminDefaultUser.Email,
                    Email = adminDefaultUser.Email,
                    SecurityStamp = adminDefaultUser.SecurityStamp,
                    Nick = adminDefaultUser.Nick
                };

                var result = await _userManager.CreateAsync(adminUser, adminDefaultUser.Password);
                await _userManager.AddToRoleAsync(adminUser, "Admin");
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(adminUser);
                await _userManager.ConfirmEmailAsync(adminUser, token);
            }
        }
    }
}
