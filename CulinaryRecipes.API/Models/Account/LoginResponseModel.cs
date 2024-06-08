namespace CulinaryRecipes.API.Models.Account
{
    public class LoginResponseModel
    {
        public string Token { get; set; }
        public string Nick { get; set; }
        public string Email { get; set; }
        public string UserId { get; set; }
    }
}
