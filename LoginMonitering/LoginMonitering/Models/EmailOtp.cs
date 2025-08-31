namespace LoginMonitering.Models
{
    public class EmailOtp
    {
        public int Id { get; set; }
        public Guid UserId { get; set; }
        public string Otp { get; set; } = null!;
        public DateTime ExpiresAt { get; set; }


        public User User { get; set; }
    }
}
