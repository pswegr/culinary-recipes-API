using CulinaryRecipes.API.Models.Account;
using CulinaryRecipes.API.Models.Identity;
using CulinaryRecipes.API.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;

namespace CulinaryRecipes.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AccountController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IConfiguration _configuration;
    private readonly IEmailService _emailService;

    public AccountController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IConfiguration configuration,
        IEmailService emailService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _configuration = configuration;
        _emailService = emailService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterModel model)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var isNicknameUnique = IsNicknameUnique(model.Nick);
        if (!isNicknameUnique)
        {
            ModelState.AddModelError("Nickname", $"{model.Nick} is already taken.");
            return BadRequest(ModelState);
        }
        var isNicknameAlphanumeric = IsNicknameAlphanumeric(model.Nick);
        if (!isNicknameAlphanumeric)
        {
            ModelState.AddModelError("Nickname", $"{model.Nick} is not alphanumeric.");
            return BadRequest(ModelState);
        }

        var user = new ApplicationUser
        {
            Nick = model.Nick,
            UserName = model.Email,
            Email = model.Email,
            PasswordHash = model.Password
        };

        var result = await _userManager.CreateAsync(user, model.Password);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(error.Code, error.Description);
            }
            return BadRequest(ModelState);
        }

        await _userManager.AddToRoleAsync(user, "User");
        await _userManager.AddClaimAsync(user, new Claim(ClaimTypes.GivenName, user.Nick));

        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

        var angularConfirmationLink = $"https://netreci.com/#/account/confirmEmail?email={user.Email}&token={Uri.EscapeDataString(token)}";

        await _emailService.SendEmailAsync(user.Email, "Confirm your email", $"Please confirm your account by clicking <a href=\"{angularConfirmationLink}\">here</a>.");
        return Ok(new { Message = "User registered successfully. Please check your email to confirm your account." });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginModel model)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, false, false);

        if (!result.Succeeded) return Unauthorized(new { Message = "Invalid login attempt" });

        var user = await _userManager.FindByEmailAsync(model.Email);

        if (user == null) return Unauthorized(new { Message = "Invalid login attempt" });

        var token = GenerateJwtToken(user);

        var loginResponse = new LoginResponseModel
        {
            Token = token,
            Email = user.Email,
            Nick = user.Nick,
            UserId = user.Id.ToString()
        };

        return Ok(loginResponse);
    }

    [HttpGet("confirmEmail")]
    public async Task<IActionResult> ConfirmEmail(string email, string token)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null) return BadRequest("Invalid email");

        var result = await _userManager.ConfirmEmailAsync(user, token);

        if (!result.Succeeded) return BadRequest("Email confirmation failed.");

        return Ok(new { Message = "Email confirmed successfully." });
    }

    [HttpPost("forgotPassword")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordModel model)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null || !await _userManager.IsEmailConfirmedAsync(user))
        {
            return BadRequest("Invalid email address or email not confirmed.");
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);

        var angularResetLink = $"https://netreci.com/#/account/resetPassword?email={user.Email}&token={Uri.EscapeDataString(token)}";

        await _emailService.SendEmailAsync(user.Email, "Reset your password", $"Reset your password by clicking <a href=\"{angularResetLink}\">here</a>.");

        return Ok(new { Message = "Password reset email sent. Please check your email." });
    }

    [HttpPost("resetPassword")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordModel model)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null) return BadRequest("Invalid email address.");

        var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(error.Code, error.Description);
            }
            return BadRequest(ModelState);
        }

        return Ok(new { Message = "Password reset successfully." });
    }

    [HttpPost("isNickValid")]
    public IActionResult IsNickValid([FromBody] ValidModel validModel)
    {
        return Ok(IsNicknameUnique(validModel.Property) && IsNicknameAlphanumeric(validModel.Property));
    }

    [HttpPost("isEmailValid")]
    public IActionResult IsEmailValid([FromBody] ValidModel validModel)
    {
        return Ok(IsEmailUnique(validModel.Property));
    }



    private string GenerateJwtToken(ApplicationUser user)
    {
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.GivenName, user.Nick)
        };
        var jwtSettings = _configuration.GetSection("Jwt").Get<JwtSettings>();
        var roles = _userManager.GetRolesAsync(user).Result;
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            jwtSettings.Issuer,
            jwtSettings.Audience,
            claims,
            expires: DateTime.Now.AddDays(7),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private bool IsNicknameUnique(string nickname)
    {
        var user = _userManager.Users.SingleOrDefault(u => u.Nick == nickname);
        return user == null;
    }

    private bool IsNicknameAlphanumeric(string nickname)
    {
        return Regex.IsMatch(nickname, "^[a-zA-Z0-9]*$");
    }

    private bool IsEmailUnique(string email)
    {
        var user = _userManager.Users.SingleOrDefault(u => u.Email == email);
        return user == null;
    }
}
