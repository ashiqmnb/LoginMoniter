namespace LoginMonitering.DTOs
{
    public class LoginAttemptDTO
    {
        public int Id { get; set; }
        public Guid? UserId { get; set; }
        public string Email { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public string? DeviceFingerprint { get; set; } 
        public DateTime AttemptedAt { get; set; } 
        public string Result { get; set; } 
    }
}
