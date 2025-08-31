namespace LoginMonitering.Models
{
    public class LoginStatus
    {
        public int Id { get; set; }
        public Guid UserId { get; set; }
        public LoginActionStatus Otp { get; set; } = LoginActionStatus.Null;
        public LoginActionStatus Captcha { get; set; } = LoginActionStatus.Null;
        public DateTime? CompletedAt { get; set; }
    }

    public enum LoginActionStatus
    {
        Null = 0,       // Not required
        Required = 1,   // Must be completed
        Completed = 2   // User finished the step
    }
}
