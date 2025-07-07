namespace nexle_api.Models.Dtos.Signup
{
    public class SignupResponse
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string DisplayName => $"{FirstName} {LastName}";
    }
}
