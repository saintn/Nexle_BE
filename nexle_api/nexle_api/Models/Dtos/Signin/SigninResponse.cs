namespace nexle_api.Models.Dtos.Signin
{
    public class SigninResponse
    {
        public UserInfoDto User { get; set; } = null!;
        public string Token { get; set; } = null!;
        public string RefreshToken { get; set; } = null!;
    }
    public class UserInfoDto
    {
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string DisplayName => $"{FirstName} {LastName}";
    }
}
