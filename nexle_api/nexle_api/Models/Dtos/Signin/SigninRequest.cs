namespace nexle_api.Models.Dtos.Signin
{
    public class SigninRequest
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}
