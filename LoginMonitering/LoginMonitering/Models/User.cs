namespace LoginMonitering.Models
{
    public class User
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Role { get; set; } = "User";


        public ICollection<EmailOtp> EmailOtps { get; set; } 
        public ICollection<LoginAttempt> LoginAttempts { get; set; } 
    }
}
