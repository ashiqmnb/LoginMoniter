namespace LoginMonitering.DTOs
{
    public class UserLoginStatsDTO
    {
        public Guid UserId { get; set; }
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public int SuccessCount { get; set; }
        public int FailedCount { get; set; }
        public int BlockedCount { get; set; }
    }
}
