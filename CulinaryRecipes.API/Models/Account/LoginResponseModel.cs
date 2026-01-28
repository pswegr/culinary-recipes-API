namespace CulinaryRecipes.API.Models.Account
{
    public class LoginResponseModel
    {
        public string Token { get; set; } = string.Empty;
        public string Nick { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
    }
}
